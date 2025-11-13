using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Aplicacion.Interfaces.Notificaciones;
using Dominio.Entidades;
using Dominio.Repositorios;
using Microsoft.Extensions.Options;

namespace Aplicacion.Servicios.Notificaciones
{
    /// <summary>
    /// Gestor de Cierre de Inspección - Sujeto Concreto del patrón Observer.
    /// 
    /// RESPONSABILIDAD PRINCIPAL:
    /// Orquestar el proceso completo de cierre de una orden de inspección, que incluye:
    /// 1. Cerrar la orden de inspección
    /// 2. Cambiar el estado del sismógrafo a "Fuera de Servicio"
    /// 3. Registrar motivos técnicos de la falla
    /// 4. Notificar a los responsables de reparación
    /// 
    /// PATRÓN DE DISEÑO:
    /// - Observer: Notifica a múltiples observadores (PantallaCCRS, Mail, WebMonitor)
    /// - Use Case: Implementa la lógica de negocio del caso de uso "Cerrar Orden"
    /// </summary>
    public class GestorCierreInspeccion : ISujetoOrdenInspeccion
    {
        #region ==================== DEPENDENCIAS INYECTADAS ====================

        private readonly IRepositorioOrdenDeInspeccion _ordenRepo;
        private readonly IRepositorioEmpleado _empleadoRepo;
        private readonly IRepositorioSismografo _sismografoRepo;
        private readonly IInicioSesionService _sesionService;
        private readonly IRepositorioEstado _estadoRepo;
        private readonly IRepositorioMotivoTipo _motivoTipoRepo;
        private readonly IOptions<SmtpSettings> _smtpSettings; // Necesario para crear InterfazNotificacionMail

        #endregion

        #region ==================== PATRÓN OBSERVER - LISTA DE OBSERVADORES ====================

        // ⚠️ IMPORTANTE: Lista estática compartida entre todas las instancias
        // Esto resuelve el problema de Scoped DI:
        // - Los observadores se crean UNA SOLA VEZ al iniciar la app (IniC1)
        // - Se almacenan en esta lista estática
        // - Cada request que crea una nueva instancia del gestor usa la misma lista
        private static readonly List<IObserverNotificacionCierre> _observadoresGlobales = new();

        #endregion

        #region ==================== ESTADO INTERNO (para notificaciones) ====================

        // Estos atributos se llenan durante el proceso de cierre
        // y se utilizan cuando se llama a Notificar() para enviar a los observadores
        private string _observacionDeCierre = string.Empty;
        private DateTime _fechaHoraCierre;
        private string[] _mailsResponsablesReparaccion = Array.Empty<string>();
        private int _idSismografo;
        private string _nombreEstado = string.Empty;
        private string[] _motivos = Array.Empty<string>();
        private string[] _comentarios = Array.Empty<string>();

        #endregion

        #region ==================== CONSTRUCTOR ====================

        public GestorCierreInspeccion(
            IRepositorioOrdenDeInspeccion ordenRepo,
            IRepositorioEmpleado empleadoRepo,
            IRepositorioSismografo sismografoRepo,
            IInicioSesionService sesionService,
            IRepositorioMotivoTipo motivoTipoRepo,
            IRepositorioEstado estadoRepo,
            IOptions<SmtpSettings> smtpSettings)
        {
            _ordenRepo = ordenRepo;
            _empleadoRepo = empleadoRepo;
            _sismografoRepo = sismografoRepo;
            _sesionService = sesionService;
            _estadoRepo = estadoRepo;
            _motivoTipoRepo = motivoTipoRepo;
            _smtpSettings = smtpSettings;
            // La lista _observadores se inicializa vacía y se llena con Suscribir()
        }

        #endregion

        #region ==================== MÉTODOS PÚBLICOS - CASOS DE USO ====================

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  INICIALIZACIÓN DEL GESTOR: iniC1()                               ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// PROPÓSITO:
        /// Configura e inicializa todos los observadores necesarios para el caso de uso.
        /// Se ejecuta al iniciar la aplicación (llamado desde Program.cs).
        /// 
        /// FLUJO DE EJECUCIÓN:
        /// 1. Crear instancia de PantallaCCRS
        /// 2. Crear instancia de InterfazNotificacionMail
        /// 3. Suscribir ambos observadores a la vez (pasando array)
        /// 
        /// RESULTADO:
        /// La lista _observadores contiene todos los observadores suscritos
        /// que serán notificados cuando se cierre una orden.
        /// </summary>
        public void IniC1()
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🔧 Inicializando GestorCierreInspeccion...");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            // Paso 1: Crear PantallaCCRS
            Console.WriteLine("\n[PASO 1] Creando PantallaCCRS...");
            var pantallaCCRS = CrearPantallaCCRS();
            
            // Paso 2: Crear InterfazNotificacionMail
            Console.WriteLine("\n[PASO 2] Creando InterfazNotificacionMail...");
            var interfazMail = CrearPantallasNotificacionMail();
            
            // Paso 3: Suscribir ambos observadores a la vez (array)
            Console.WriteLine("\n[PASO 3] Suscribiendo observadores...");
            Suscribir(new IObserverNotificacionCierre[] { pantallaCCRS, interfazMail });
            
            Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"✅ Gestor inicializado con {_observadoresGlobales.Count} observadores suscritos.");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
        }

        /// <summary>
        /// CONSULTA: Obtiene todas las órdenes que pueden ser cerradas.
        /// Filtra por: empleado logueado + estado "Completada".
        /// 
        /// FLUJO:
        /// 1. Obtener usuario logueado
        /// 2. Obtener todas las órdenes
        /// 3. Filtrar las que pertenecen al empleado y están completadas
        /// </summary>
        public async Task<IEnumerable<OrdenDeInspeccion>> BuscarOrdenesDeInspeccion()
        {
            var usuario = _sesionService.ObtenerUsuarioLogueado();
            if (usuario == null)
                return Enumerable.Empty<OrdenDeInspeccion>();

            var empleado = usuario.GetRILogueado();
            var ordenes = await _ordenRepo.ObtenerTodasAsync();

            // Filtra solo las órdenes del empleado actual y completadas
            var cerrables = ordenes
                .Where(o => o.EsDeEmpleado(empleado) && o.EstaCompletamenteRealizada())
                .ToList();

            return cerrables;
        }

        /// <summary>
        /// CONSULTA: Obtiene los tipos de motivos disponibles para reportar fallas.
        /// Estos motivos se usan en el formulario de cierre (dropdown de motivos).
        /// </summary>
        public async Task<IEnumerable<MotivoTipo>> ObtenerMotivosAsync()
        {
            var motivos = await _motivoTipoRepo.ObtenerTodosAsync();
            return motivos ?? Enumerable.Empty<MotivoTipo>();
        }

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  MÉTODO PRINCIPAL: CERRAR ORDEN DE INSPECCIÓN                      ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// SECUENCIA COMPLETA DE EJECUCIÓN (7 PASOS):
        /// 
        /// PASO 1: Validaciones previas
        ///         - Usuario logueado existe
        ///         - Orden existe en BD
        ///         - Observación no vacía
        ///         - Usuario confirmó la acción
        ///         - Orden no está ya cerrada
        ///         - Al menos un motivo seleccionado
        /// 
        /// PASO 2: Buscar estado "Cerrada" para OrdenInspeccion
        ///         - Consulta en tabla Estados
        ///         - Ámbito: "OrdenInspeccion", Nombre: "Cerrada"
        /// 
        /// PASO 3: Cerrar la orden
        ///         - Cambiar estado de la orden a "Cerrada"
        ///         - Registrar observación del cierre
        /// 
        /// PASO 4: Actualizar el sismógrafo relacionado
        ///         - Cambiar estado a "Fuera de Servicio"
        ///         - Crear CambioEstado con motivos técnicos
        ///         - Marcar como "En Reparación"
        /// 
        /// PASO 5: Guardar cambios en la base de datos
        ///         - Persistir OrdenInspeccion
        ///         - Persistir Sismografo
        ///         - Persistir CambioEstado
        ///         - Persistir MotivosFueraServicio
        /// 
        /// PASO 6: Obtener emails de responsables de reparación
        ///         - Filtrar empleados con rol "ResponsableReparacion"
        ///         - Extraer sus direcciones de correo
        /// 
        /// PASO 7: Notificar a todos los observadores (Patrón Observer)
        ///         - PantallaCCRS (muestra en consola)
        ///         - InterfazNotificacionMail (envía emails)
        ///         - ObservadorWebMonitor (registra en log)
        /// 
        /// RETORNO: Mensaje de éxito o error
        /// </summary>
        public async Task<string> CerrarOrdenInspeccion(CierreOrdenRequest request)
        {
            // ═══════════════════════════════════════════════════════════════
            // PASO 1: VALIDACIONES PREVIAS
            // ═══════════════════════════════════════════════════════════════
            
            // Verificar que haya un usuario logueado
            var usuario = _sesionService.ObtenerUsuarioLogueado();
            if (usuario == null)
                return "No hay usuario logueado.";

            // Buscar la orden en la base de datos
            var ordenEntidad = await _ordenRepo.BuscarPorNroAsync(request.NroOrden);
            if (ordenEntidad == null)
                return $"No se encontró la orden {request.NroOrden}.";

            // Validar que se ingresó una observación
            if (string.IsNullOrWhiteSpace(request.Observacion))
                return "Debe ingresar una observación.";

            // Validar confirmación del usuario
            if (!request.Confirmar)
                return "Cierre cancelado por el usuario.";

            // Validar que la orden no esté ya cerrada
            if (ordenEntidad.GetEstado()?.EsCerrada() == true)
                return "La orden ya está cerrada.";

            // Validar que haya al menos un motivo
            if (request.MotivosTipo == null || request.MotivosTipo.Count == 0)
                return "Debe seleccionar al menos un motivo.";

            // Obtener catálogo de motivos para validación y asociación posterior
            var motivosTiposRepo = await _motivoTipoRepo.ObtenerTodosAsync();

            // Guardar observación y timestamp del cierre
            _observacionDeCierre = request.Observacion;
            _fechaHoraCierre = DateTime.Now;

            // ═══════════════════════════════════════════════════════════════
            // PASO 2: BUSCAR ESTADO "CERRADA" PARA ORDEN DE INSPECCIÓN
            // ═══════════════════════════════════════════════════════════════
            var estadoCerrado = BuscarEstadoCerradoParaOrdenInspeccion();

            // ═══════════════════════════════════════════════════════════════
            // PASO 3: CERRAR LA ORDEN (cambio de estado en entidad)
            // ═══════════════════════════════════════════════════════════════
            try
            {
                ordenEntidad.Cerrar(request.Observacion, estadoCerrado);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            // ═══════════════════════════════════════════════════════════════
            // PASO 4: ACTUALIZAR SISMÓGRAFO RELACIONADO
            // (Cambiar a "Fuera de Servicio" y registrar motivos)
            // ═══════════════════════════════════════════════════════════════
            var estacion = ordenEntidad.GetEstacion();
            var sismografo = estacion?.ObtenerIdSismografo();

            if (sismografo != null && estacion != null)
            {
                await RegistrarFallaSismografo(
                    sismografo.GetIdentificadorSismografo(),
                    estacion,
                    request.MotivosTipo,
                    request.Comentarios,
                    motivosTiposRepo);
            }

            // ═══════════════════════════════════════════════════════════════
            // PASO 5: GUARDAR CAMBIOS EN BASE DE DATOS
            // ═══════════════════════════════════════════════════════════════
            _ordenRepo.Actualizar(ordenEntidad);
            await _ordenRepo.GuardarCambiosAsync();

            // ═══════════════════════════════════════════════════════════════
            // PASO 6: OBTENER EMAILS DE RESPONSABLES DE REPARACIÓN
            // ═══════════════════════════════════════════════════════════════
            var empleados = await (_empleadoRepo.ObtenerTodosAsync()) ?? new List<Empleado>();
            var mailsResp = ObtenerMailsResponsablesReparacion(empleados);
            _mailsResponsablesReparaccion = mailsResp.ToArray();

            // ═══════════════════════════════════════════════════════════════
            // PASO 7: NOTIFICAR A TODOS LOS OBSERVADORES (PATRÓN OBSERVER)
            // ═══════════════════════════════════════════════════════════════
            Notificar();

            return $"Orden {ordenEntidad.GetNroOrden()} cerrada correctamente. Notificaciones enviadas.";
        }

        #endregion

        #region ==================== MÉTODOS PRIVADOS - LÓGICA DE NEGOCIO ====================

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  REGISTRAR FALLA DEL SISMÓGRAFO (actualizarIdSismografo)          ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// PROPÓSITO:
        /// Cambia el estado del sismógrafo de "En Operación" a "Fuera de Servicio",
        /// registra los motivos técnicos de la falla y lo marca como "En Reparación".
        /// 
        /// NOTA: El nombre del método viene del diagrama UML original, pero en realidad
        /// NO actualiza el ID, sino el ESTADO del sismógrafo.
        /// 
        /// FLUJO DETALLADO (9 PASOS):
        /// 
        /// 1. Obtener sismógrafo por identificación
        ///    - Busca en BD por identificador (ej: "SISMO-001")
        /// 
        /// 2. Extraer ID numérico para notificaciones
        ///    - "SISMO-001" → 1
        ///    - Este ID se usa solo para los observadores
        /// 
        /// 3. Validar relación con estación
        ///    - Verifica que el sismógrafo pertenece a la estación
        /// 
        /// 4. Buscar estado "FueraDeServicio"
        ///    - Consulta tabla Estados (Ámbito: Sismografo)
        /// 
        /// 5. Cambiar estado del sismógrafo
        ///    - SetEstadoActual("FueraDeServicio")
        /// 
        /// 6. Crear registro de cambio de estado
        ///    - Nueva entidad CambioEstado con timestamp
        /// 
        /// 7. Registrar motivos técnicos de la falla
        ///    - Loop: Por cada motivo seleccionado
        ///      * Crear MotivoFueraServicio
        ///      * Asociar comentario técnico
        ///      * Agregar a CambioEstado
        /// 
        /// 8. Marcar como "En Reparación"
        ///    - EnviarAReparar() cambia estado a "EnReparacion"
        /// 
        /// 9. Persistir cambios en BD
        ///    - Actualiza Sismografo con todos sus cambios
        /// </summary>
        private async Task RegistrarFallaSismografo(
            string identificacionSismografo,
            EstacionSismologica estacion,
            List<string> motivosTipo,
            List<string> comentarios,
            IEnumerable<MotivoTipo> motivosTiposRepo)
        {
            // ─────────────────────────────────────────────────────────────
            // 1. Obtener el sismógrafo desde la base de datos
            // ─────────────────────────────────────────────────────────────
            var sismografoPersistido = await _sismografoRepo.ObtenerPorIdentificacionAsync(
                identificacionSismografo
            );

            if (sismografoPersistido == null)
                return;

            // ─────────────────────────────────────────────────────────────
            // 2. Extraer ID numérico para las notificaciones
            //    Ejemplo: "SISMO-001" → 1
            // ─────────────────────────────────────────────────────────────
            _idSismografo = ExtraerIdNumerico(sismografoPersistido.GetIdentificadorSismografo());

            // ─────────────────────────────────────────────────────────────
            // 3. Validar que el sismógrafo pertenece a la estación
            // ─────────────────────────────────────────────────────────────
            if (!sismografoPersistido.SosDeEstacionSismologica(estacion))
                return;

            // ─────────────────────────────────────────────────────────────
            // 4. Buscar estado "FueraDeServicio" en la BD
            // ─────────────────────────────────────────────────────────────
            var estadoFueraServicio = BuscarEstadoSismografoFueraDeServicio();

            // ─────────────────────────────────────────────────────────────
            // 5. Cambiar estado del sismógrafo a "Fuera de Servicio"
            // ─────────────────────────────────────────────────────────────
            SetNombreEstado(estadoFueraServicio.GetNombre()); // Guardar para notificaciones
            sismografoPersistido.SetEstadoActual(estadoFueraServicio);

            // ─────────────────────────────────────────────────────────────
            // 6. Crear registro de cambio de estado (entidad CambioEstado)
            //    Incluye timestamp de inicio y fin
            // ─────────────────────────────────────────────────────────────
            var cambio = sismografoPersistido.CrearCambioEstado(estadoFueraServicio);
            SetFechaHoraCierre(_fechaHoraCierre); // Guardar para notificaciones
            cambio.SetFechaHoraFin();

            // ─────────────────────────────────────────────────────────────
            // 7. LOOP: Registrar cada motivo técnico de la falla
            //    Ejemplo: 
            //    - Motivo 1: "Falla eléctrica" → "Cortocircuito en panel"
            //    - Motivo 2: "Cable dañado" → "Cable de alimentación cortado"
            // ─────────────────────────────────────────────────────────────
            var motivosLista = new List<string>();
            var comentariosLista = new List<string>();

            foreach (var tipo in motivosTipo)
            {
                // Buscar el tipo de motivo en el catálogo
                var tipoEncontrado = motivosTiposRepo.FirstOrDefault(
                    m => m.TipoMotivo == tipo || m.Descripcion == tipo
                );

                if (tipoEncontrado != null)
                {
                    // Obtener el comentario correspondiente (si existe)
                    var comentario = comentarios.ElementAtOrDefault(
                        motivosTipo.IndexOf(tipo)
                    ) ?? string.Empty;

                    // Crear entidad MotivoFueraServicio
                    var motivo = new MotivoFueraServicio(tipoEncontrado, comentario);
                    cambio.CrearMotivosFueraDeServicio(motivo);

                    // Guardar para notificaciones
                    motivosLista.Add(tipoEncontrado.TipoMotivo);
                    comentariosLista.Add(comentario);
                }
            }

            // Guardar motivos y comentarios para las notificaciones
            SetMotivos(motivosLista.ToArray());
            SetComentarios(comentariosLista.ToArray());

            // ─────────────────────────────────────────────────────────────
            // 8. Marcar el sismógrafo como "En Reparación"
            //    IMPORTANTE: Buscar el estado existente y pasarlo como parámetro
            //    (no crear uno nuevo para evitar violación de PK)
            // ─────────────────────────────────────────────────────────────
            var estadoEnReparacion = await _estadoRepo.ObtenerPorAmbitoYNombreAsync("Sismografo", "EnReparacion");
            if (estadoEnReparacion == null)
                throw new InvalidOperationException("Estado 'EnReparacion' para 'Sismografo' no encontrado.");
            
            sismografoPersistido.EnviarAReparar(estadoEnReparacion);

            // ─────────────────────────────────────────────────────────────
            // 9. NO guardar aquí - EF Core tracking guardará todo junto
            //    cuando se llame a GuardarCambiosAsync() de la orden
            // ─────────────────────────────────────────────────────────────
            // await _sismografoRepo.ActualizarAsync(sismografoPersistido); // ❌ ELIMINADO
        }

        /// <summary>
        /// Busca el estado "Cerrada" para una Orden de Inspección.
        /// Se ejecuta antes de cerrar la orden.
        /// </summary>
        private Estado BuscarEstadoCerradoParaOrdenInspeccion()
        {
            return _estadoRepo.ObtenerPorAmbitoYNombreAsync("OrdenInspeccion", "Cerrada").Result
                ?? throw new InvalidOperationException("Estado 'Cerrada' para 'OrdenInspeccion' no encontrado.");
        }

        /// <summary>
        /// Busca el estado "FueraDeServicio" para un Sismografo.
        /// Se ejecuta durante el registro de falla del sismógrafo.
        /// </summary>
        private Estado BuscarEstadoSismografoFueraDeServicio()
        {
            return _estadoRepo.ObtenerPorAmbitoYNombreAsync("Sismografo", "FueraDeServicio").Result
                ?? throw new InvalidOperationException("Estado 'FueraDeServicio' para 'Sismografo' no encontrado.");
        }

        /// <summary>
        /// Obtiene los emails de todos los empleados con rol "ResponsableReparacion".
        /// Estos emails se usan para enviar notificaciones por mail.
        /// 
        /// Loop: Recorre todos los empleados y filtra por rol.
        /// </summary>
        private List<string> ObtenerMailsResponsablesReparacion(IEnumerable<Empleado> empleados)
        {
            var mails = new List<string>();
            Console.WriteLine($"[DEBUG] Total empleados recibidos: {empleados.Count()}");
            
            // Loop: Recorrer empleados y filtrar responsables de reparación
            foreach (var emp in empleados)
            {
                Console.WriteLine($"[DEBUG] Empleado: {emp.GetNombreCompleto()} - Roles: {emp.Roles.Count}");
                if (emp.EsResponsableDeReparacion())
                {
                    var mail = emp.ObtenerMail();
                    mails.Add(mail);
                    Console.WriteLine($"[DEBUG] ✅ Responsable encontrado: {emp.GetNombreCompleto()} ({mail})");
                }
            }
            
            Console.WriteLine($"[DEBUG] Total responsables de reparación encontrados: {mails.Count}");
            return mails;
        }

        /// <summary>
        /// Extrae el número de un identificador de sismógrafo.
        /// Ejemplo: "SISMO-001" → 1
        /// Se usa para las notificaciones (los observadores reciben el ID numérico).
        /// </summary>
        private int ExtraerIdNumerico(string identificacion)
        {
            if (string.IsNullOrEmpty(identificacion))
                return 0;

            // Intentar extraer número del formato "SISMO-001" -> 1
            var partes = identificacion.Split('-');
            if (partes.Length > 1 && int.TryParse(partes[^1], out int numero))
                return numero;

            // Fallback: usar hash code si no se puede extraer
            return Math.Abs(identificacion.GetHashCode()) % 10000;
        }

        #endregion

        #region ==================== SETTERS PARA ESTADO INTERNO (notificaciones) ====================

        /// <summary>
        /// Guarda el nombre del estado para las notificaciones.
        /// Ejemplo: "FueraDeServicio"
        /// </summary>
        private void SetNombreEstado(string nombreEstado)
        {
            _nombreEstado = nombreEstado;
        }

        /// <summary>
        /// Guarda la fecha/hora del cierre para las notificaciones.
        /// </summary>
        private void SetFechaHoraCierre(DateTime fechaHoraCierre)
        {
            _fechaHoraCierre = fechaHoraCierre;
        }

        /// <summary>
        /// Guarda los motivos técnicos para las notificaciones.
        /// Ejemplo: ["Falla eléctrica", "Cable dañado"]
        /// </summary>
        private void SetMotivos(string[] motivos)
        {
            _motivos = motivos ?? Array.Empty<string>();
        }

        /// <summary>
        /// Guarda los comentarios de cada motivo para las notificaciones.
        /// Ejemplo: ["Cortocircuito en panel", "Cable de alimentación cortado"]
        /// </summary>
        private void SetComentarios(string[] comentarios)
        {
            _comentarios = comentarios ?? Array.Empty<string>();
        }

        #endregion

        #region ==================== PATRÓN OBSERVER - GESTIÓN DE OBSERVADORES ====================

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  CREAR PANTALLA CCRS (Cierre de Orden)                            ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// PROPÓSITO:
        /// Crea una nueva instancia del observador PantallaCCRS.
        /// Este observador mostrará en consola los detalles del cierre de la orden.
        /// 
        /// PATRÓN:
        /// Factory Method - crea y retorna una nueva instancia del observador.
        /// 
        /// RETORNO:
        /// Instancia de PantallaCCRS lista para ser suscrita.
        /// </summary>
        public IObserverNotificacionCierre CrearPantallaCCRS()
        {
            Console.WriteLine("📋 Creando PantallaCCRS...");
            var pantalla = new PantallaCCRS();
            return pantalla;
        }

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  CREAR INTERFAZ NOTIFICACIÓN MAIL                                 ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// PROPÓSITO:
        /// Crea una nueva instancia del observador InterfazNotificacionMail.
        /// Este observador enviará emails a los responsables de reparación.
        /// 
        /// PATRÓN:
        /// Factory Method - crea y retorna una nueva instancia del observador.
        /// 
        /// RETORNO:
        /// Instancia de InterfazNotificacionMail lista para ser suscrita.
        /// </summary>
        public IObserverNotificacionCierre CrearPantallasNotificacionMail()
        {
            Console.WriteLine("📧 Creando InterfazNotificacionMail...");
            var interfazMail = new InterfazNotificacionMail(_smtpSettings);
            return interfazMail;
        }

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  SUSCRIBIR OBSERVADORES (PATRÓN OBSERVER)                         ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// PROPÓSITO:
        /// Añade múltiples observadores a la lista estática compartida.
        /// Esta lista persiste durante toda la vida de la aplicación.
        /// 
        /// FLUJO:
        /// 1. Recibe un array de observadores como parámetro
        /// 2. Loop: Por cada observador en el array
        ///    - Valida que no sea nulo
        ///    - Lo añade a la lista estática _observadoresGlobales
        /// 3. Confirma las suscripciones por consola
        /// 
        /// PARÁMETROS:
        /// - observadores: Array de IObserverNotificacionCierre a suscribir
        /// 
        /// NOTA: La lista es estática para que persista entre requests HTTP
        /// </summary>
        public void Suscribir(IObserverNotificacionCierre[] observadores)
        {
            if (observadores == null || observadores.Length == 0)
            {
                Console.WriteLine("⚠️ Intento de suscribir un array vacío o nulo.");
                return;
            }

            // Loop: Recorrer el array y añadir cada observador a la lista estática
            foreach (var observador in observadores)
            {
                if (observador == null)
                {
                    Console.WriteLine("⚠️ Observador nulo encontrado en el array, se omite.");
                    continue;
                }

                // Añadir a la lista estática compartida
                _observadoresGlobales.Add(observador);
                
                Console.WriteLine($"✅ Observador '{observador.GetType().Name}' suscrito correctamente.");
            }
            
            Console.WriteLine($"   📊 Total de observadores suscritos: {_observadoresGlobales.Count}");
        }

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  QUITAR OBSERVADOR (PATRÓN OBSERVER)                              ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// PROPÓSITO:
        /// Remueve un observador de la lista estática compartida.
        /// Permite desuscribir observadores en tiempo de ejecución.
        /// 
        /// PARÁMETROS:
        /// - observador: Instancia de IObserverNotificacionCierre a desuscribir
        /// </summary>
        public void Quitar(IObserverNotificacionCierre observador)
        {
            if (observador == null)
            {
                Console.WriteLine("⚠️ Intento de quitar un observador nulo.");
                return;
            }

            // Remover de la lista estática compartida
            bool removido = _observadoresGlobales.Remove(observador);
            
            if (removido)
            {
                Console.WriteLine($"❌ Observador '{observador.GetType().Name}' desuscrito correctamente.");
                Console.WriteLine($"   Total de observadores suscritos: {_observadoresGlobales.Count}");
            }
            else
            {
                Console.WriteLine($"⚠️ Observador '{observador.GetType().Name}' no estaba suscrito.");
            }
        }

        /// <summary>
        /// ╔════════════════════════════════════════════════════════════════════╗
        /// ║  NOTIFICAR A TODOS LOS OBSERVADORES (PATRÓN OBSERVER)             ║
        /// ╚════════════════════════════════════════════════════════════════════╝
        /// 
        /// PROPÓSITO:
        /// Envía una notificación a todos los observadores suscritos con la información
        /// del cierre de la orden.
        /// 
        /// OBSERVADORES ACTUALES:
        /// 1. PantallaCCRS: Muestra en consola los detalles del cierre
        /// 2. InterfazNotificacionMail: Envía emails a los responsables de reparación
        /// 3. ObservadorWebMonitor: Registra el evento en un log estático
        /// 
        /// DATOS ENVIADOS:
        /// - ID del sismógrafo (numérico)
        /// - Nombre del nuevo estado ("FueraDeServicio")
        /// - Fecha/hora del cierre
        /// - Motivos técnicos (array de strings)
        /// - Comentarios de cada motivo (array de strings)
        /// - Emails de responsables de reparación (array de strings)
        /// 
        /// Loop: Recorre todos los observadores inyectados y llama a Actualizar()
        /// </summary>
        public void Notificar()
        {
            Console.WriteLine($"\n🔔 [NOTIFICAR] Iniciando notificación a {_observadoresGlobales.Count} observadores...");
            Console.WriteLine($"   - ID Sismógrafo: {_idSismografo}");
            Console.WriteLine($"   - Estado: {_nombreEstado}");
            Console.WriteLine($"   - Motivos: {string.Join(", ", _motivos)}");
            
            // Loop: Recorrer todos los observadores estáticos y notificar uno por uno
            int contador = 1;
            foreach (var observador in _observadoresGlobales)
            {
                try
                {
                    Console.WriteLine($"\n   [{contador}/{_observadoresGlobales.Count}] Notificando a {observador.GetType().Name}...");
                    
                    // Llamar al método Actualizar() de cada observador
                    // con todos los datos del cierre
                    observador.Actualizar(
                        idSismografo: _idSismografo,
                        nombreEstado: _nombreEstado,
                        fechaHoraCierre: _fechaHoraCierre,
                        motivos: _motivos,
                        comentarios: _comentarios,
                        mailsResponsablesReparacion: _mailsResponsablesReparaccion
                    );
                    
                    Console.WriteLine($"   ✅ {observador.GetType().Name} notificado correctamente.");
                }
                catch (Exception ex)
                {
                    // Si un observador falla, no detener el proceso
                    // (permite que otros observadores sigan funcionando)
                    Console.WriteLine($"   ❌ Error al notificar {observador.GetType().Name}: {ex.Message}");
                }
                contador++;
            }
            
            Console.WriteLine($"\n✅ Notificación completada a todos los observadores.\n");
        }

        #endregion
    }
}
