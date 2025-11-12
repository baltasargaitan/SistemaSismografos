using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Aplicacion.Interfaces.Notificaciones;
using Dominio.Entidades;
using Dominio.Repositorios;

namespace Aplicacion.Servicios.Notificaciones
{
    /// <summary>
    /// Gestor de Cierre de Inspección - Sujeto Concreto del patrón Observer.
    /// Fusiona la lógica del caso de uso de cerrar orden con la gestión de observadores.
    /// Implementa fielmente el diagrama UML proporcionado.
    /// </summary>
    public class GestorCierreInspeccion : ISujetoOrdenInspeccion
    {
        // ========== Dependencias inyectadas ==========
        private readonly IRepositorioOrdenDeInspeccion _ordenRepo;
        private readonly IRepositorioEmpleado _empleadoRepo;
        private readonly IRepositorioSismografo _sismografoRepo;
        private readonly IInicioSesionService _sesionService;
        private readonly IRepositorioEstado _estadoRepo;
        private readonly IRepositorioMotivoTipo _motivoTipoRepo;
        private readonly IEnumerable<IObserverNotificacionCierre> _observadores;

        // ========== Atributos del Sujeto según diagrama ==========
        private string _observacionDeCierre = string.Empty;
        private DateTime _fechaHoraCierre;
        private string[] _mailsResponsablesReparaccion = Array.Empty<string>();
        private int _idSismografo;
        private string _nombreEstado = string.Empty;
        private string[] _motivos = Array.Empty<string>();
        private string[] _comentarios = Array.Empty<string>();

        // ========== Constructor ==========
        public GestorCierreInspeccion(
            IRepositorioOrdenDeInspeccion ordenRepo,
            IRepositorioEmpleado empleadoRepo,
            IRepositorioSismografo sismografoRepo,
            IInicioSesionService sesionService,
            IRepositorioMotivoTipo motivoTipoRepo,
            IRepositorioEstado estadoRepo,
            IEnumerable<IObserverNotificacionCierre> observadores)
        {
            _ordenRepo = ordenRepo;
            _empleadoRepo = empleadoRepo;
            _sismografoRepo = sismografoRepo;
            _sesionService = sesionService;
            _estadoRepo = estadoRepo;
            _motivoTipoRepo = motivoTipoRepo;
            _observadores = observadores;
        }

        // ==========================================================
        // IMPLEMENTACIÓN DE ISujetoOrdenInspeccion (Patrón Observer)
        // ==========================================================

        /// <summary>
        /// Suscribe un observador a la lista de notificaciones.
        /// Nota: Con inyección de observadores, este método es opcional.
        /// </summary>
        public void Suscribir(IObserverNotificacionCierre observador)
        {
            // Los observadores se inyectan automáticamente vía DI
            Console.WriteLine($"✅ Observador {observador?.GetType().Name} disponible vía DI.");
        }

        /// <summary>
        /// Quita un observador de la lista de notificaciones.
        /// Nota: Con inyección de observadores, este método es opcional.
        /// </summary>
        public void Quitar(IObserverNotificacionCierre observador)
        {
            Console.WriteLine($"❌ No se puede quitar observador (inyectados vía DI).");
        }

        /// <summary>
        /// Notifica a todos los observadores suscritos.
        /// Loop: Recorrer Miembros [Mientras haya miembros]
        /// </summary>
        public void Notificar()
        {
            // Loop: Recorrer los observadores y notificar uno por uno
            foreach (var observador in _observadores)
            {
                try
                {
                    // Llamada según firma del diagrama con datos reales
                    observador.Actualizar(
                        idSismografo: _idSismografo,
                        nombreEstado: _nombreEstado,
                        fechaHoraCierre: _fechaHoraCierre,
                        motivos: _motivos,
                        comentarios: _comentarios,
                        mailsResponsablesReparacion: _mailsResponsablesReparaccion
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Error al notificar observador {observador.GetType().Name}: {ex.Message}");
                }
            }
        }

        // ==========================================================
        // MÉTODOS DEL CASO DE USO (según diagrama)
        // ==========================================================

        /// <summary>
        /// iniC1() - Inicialización del gestor.
        /// </summary>
        public void IniC1()
        {
            Console.WriteLine("🔧 Inicializando GestorCierreInspeccion...");
            CrearPantallaCCRS();
            CrearPantallasNotificacionMail();
        }

        /// <summary>
        /// crearPantallasCC RS():void
        /// Crea e inicializa la pantalla CCRS (Cierre de Orden).
        /// </summary>
        public void CrearPantallaCCRS()
        {
            // Este método es para inicializar observadores específicos
            // En tu implementación actual, esto se hace en Program.cs con DI
            Console.WriteLine("📋 PantallaCCRS creada.");
        }

        /// <summary>
        /// crearPantallasNotificacionM():void
        /// Crea e inicializa la pantalla de notificación por mail.
        /// </summary>
        public void CrearPantallasNotificacionMail()
        {
            Console.WriteLine("📧 InterfazNotificacionMail creada.");
        }

        /// <summary>
        /// cerrarOrdenInspeccion(): void
        /// Método principal del caso de uso según el diagrama de secuencia.
        /// FLUJO SEGÚN DIAGRAMA:
        /// 1. Validaciones previas
        /// 2. buscarEstadoCerradoParaOrdenInspeccion()
        /// 3. Cerrar orden (cambio de estado)
        /// 4. actualizarIdSismografo() - con todos sus setters internos
        /// 5. Guardar cambios en BD
        /// 6. obtenerMailsResponsablesReparacion()
        /// 7. notificar() - Patrón Observer (rediseño aplicado)
        /// </summary>
        public async Task<string> CerrarOrdenInspeccion(CierreOrdenRequest request)
        {
            // =====================================================
            // PASO 1: Validaciones previas
            // =====================================================
            var usuario = _sesionService.ObtenerUsuarioLogueado();
            if (usuario == null)
                return "No hay usuario logueado.";

            var ordenEntidad = await _ordenRepo.BuscarPorNroAsync(request.NroOrden);
            if (ordenEntidad == null)
                return $"No se encontró la orden {request.NroOrden}.";

            if (string.IsNullOrWhiteSpace(request.Observacion))
                return "Debe ingresar una observación.";

            if (!request.Confirmar)
                return "Cierre cancelado por el usuario.";

            if (ordenEntidad.GetEstado()?.EsCerrada() == true)
                return "La orden ya está cerrada.";

            if (request.MotivosTipo == null || request.MotivosTipo.Count == 0)
                return "Debe seleccionar al menos un motivo.";

            // Obtener motivos del repositorio para validación posterior
            var motivosTiposRepo = await _motivoTipoRepo.ObtenerTodosAsync();

            // Guardar observación y fecha de cierre
            _observacionDeCierre = request.Observacion;
            _fechaHoraCierre = DateTime.Now;

            // =====================================================
            // PASO 2: buscarEstadoCerradoParaOrdenInspeccion()
            // Según diagrama: se busca el estado ANTES de cerrar
            // =====================================================
            var estadoCerrado = BuscarEstadoCerradoParaOrdenInspeccion();

            // =====================================================
            // PASO 3: Cerrar la orden (cambio de estado)
            // ordenEntidad.Cerrar() según el diagrama
            // =====================================================
            try
            {
                ordenEntidad.Cerrar(request.Observacion, estadoCerrado);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            // =====================================================
            // PASO 4: actualizarIdSismografo()
            // Según diagrama: después de cerrar orden
            // Loop Recorrer Miembros [Mientras haya miembros]
            // =====================================================
            var estacion = ordenEntidad.GetEstacion();
            var sismografo = estacion?.ObtenerIdSismografo();

            if (sismografo != null && estacion != null)
            {
                await ActualizarIdSismografo(
                    sismografo.GetIdentificadorSismografo(),
                    estacion,
                    request.MotivosTipo,
                    request.Comentarios,
                    motivosTiposRepo);
            }

            // =====================================================
            // PASO 5: Guardar cambios en BD
            // Según diagrama: después de actualizar sismógrafo
            // =====================================================
            _ordenRepo.Actualizar(ordenEntidad);
            await _ordenRepo.GuardarCambiosAsync();

            // =====================================================
            // PASO 6: obtenerMailsResponsablesReparacion()
            // Según diagrama: antes de notificar
            // =====================================================
            var empleados = await (_empleadoRepo.ObtenerTodosAsync()) ?? new List<Empleado>();
            var mailsResp = ObtenerMailsResponsablesReparacion(empleados);
            _mailsResponsablesReparaccion = mailsResp.ToArray();

            // =====================================================
            // PASO 7: notificar()
            // PATRÓN OBSERVER (rediseño aplicado - no modificar)
            // Loop: Recorrer observadores y llamar actualizar()
            // =====================================================
            Notificar();

            return $"Orden {ordenEntidad.GetNroOrden()} cerrada correctamente. Notificaciones enviadas.";
        }

        /// <summary>
        /// actualizarIdSismografo(...): void
        /// Método según diagrama para actualizar el sismógrafo.
        /// FLUJO SEGÚN DIAGRAMA:
        /// 1. Obtener sismógrafo por identificación
        /// 2. Validar relación con estación (sosDeEstacionSismologica)
        /// 3. buscarEstadoSismografoFueraDeServicio()
        /// 4. setNombreEstado()
        /// 5. crearCambioEstado()
        /// 6. setFechaHoraCierre()
        /// 7. Loop: setMotivos() y setComentarios()
        /// 8. enviarAReparar()
        /// 9. Persistir cambios
        /// </summary>
        private async Task ActualizarIdSismografo(
            string identificacionSismografo,
            EstacionSismologica estacion,
            List<string> motivosTipo,
            List<string> comentarios,
            IEnumerable<MotivoTipo> motivosTiposRepo)
        {
            // PASO 1: Obtener el sismógrafo persistido por su identificador
            var sismografoPersistido = await _sismografoRepo.ObtenerPorIdentificacionAsync(
                identificacionSismografo
            );

            if (sismografoPersistido == null)
                return;

            // Guardar ID del sismógrafo para notificación (extraer número de formato "SISMO-XXX")
            _idSismografo = ExtraerIdNumerico(sismografoPersistido.GetIdentificadorSismografo());

            // PASO 2: Validar relación con estación según diagrama
            // sosDeEstacionSismologica(estacion: EstacionSismologica): bool
            if (sismografoPersistido.SosDeEstacionSismologica(estacion))
            {
                // PASO 3: buscarEstadoSismografoFueraDeServicio()
                var estadoFueraServicio = BuscarEstadoSismografoFueraDeServicio();

                // PASO 4: setNombreEstado(nombreEstado: String): void
                SetNombreEstado(estadoFueraServicio.GetNombre());
                sismografoPersistido.SetEstadoActual(estadoFueraServicio);

                // PASO 5: crearCambioEstado()
                var cambio = sismografoPersistido.CrearCambioEstado(estadoFueraServicio);

                // PASO 6: setFechaHoraCierre(fechaHoraCierre: DateTime): void
                SetFechaHoraCierre(_fechaHoraCierre);
                cambio.SetFechaHoraFin();

                // PASO 7: Loop - setMotivos() y setComentarios()
                var motivosLista = new List<string>();
                var comentariosLista = new List<string>();

                // Loop: Recorrer Miembros [Mientras haya miembros]
                foreach (var tipo in motivosTipo)
                {
                    var tipoEncontrado = motivosTiposRepo.FirstOrDefault(
                        m => m.TipoMotivo == tipo || m.Descripcion == tipo
                    );

                    if (tipoEncontrado != null)
                    {
                        var comentario = comentarios.ElementAtOrDefault(
                            motivosTipo.IndexOf(tipo)
                        ) ?? string.Empty;

                        var motivo = new MotivoFueraServicio(tipoEncontrado, comentario);
                        cambio.CrearMotivosFueraDeServicio(motivo);

                        // Guardar para notificación
                        motivosLista.Add(tipoEncontrado.TipoMotivo);
                        comentariosLista.Add(comentario);
                    }
                }

                // Actualizar atributos para Notificar() - Llamadas a setters según diagrama
                SetMotivos(motivosLista.ToArray());
                SetComentarios(comentariosLista.ToArray());

                // PASO 8: enviarAReparar()
                sismografoPersistido.EnviarAReparar();

                // PASO 9: Persistir el sismógrafo modificado
                await _sismografoRepo.ActualizarAsync(sismografoPersistido);
            }
        }

        // ==========================================================
        // MÉTODOS SETTER EXPLÍCITOS (según diagrama de secuencia)
        // ==========================================================

        /// <summary>
        /// setNombreEstado(nombreEstado: String): void
        /// Setter explícito según diagrama UML de secuencia.
        /// </summary>
        private void SetNombreEstado(string nombreEstado)
        {
            _nombreEstado = nombreEstado;
        }

        /// <summary>
        /// setFechaHoraCierre(fechaHoraCierre: DateTime): void
        /// Setter explícito según diagrama UML de secuencia.
        /// </summary>
        private void SetFechaHoraCierre(DateTime fechaHoraCierre)
        {
            _fechaHoraCierre = fechaHoraCierre;
        }

        /// <summary>
        /// setMotivos(motivos: String[]): void
        /// Setter explícito según diagrama UML de secuencia.
        /// </summary>
        private void SetMotivos(string[] motivos)
        {
            _motivos = motivos ?? Array.Empty<string>();
        }

        /// <summary>
        /// setComentarios(comentarios: String[]): void
        /// Setter explícito según diagrama UML de secuencia.
        /// </summary>
        private void SetComentarios(string[] comentarios)
        {
            _comentarios = comentarios ?? Array.Empty<string>();
        }

        /// <summary>
        /// Extrae el número de un identificador de sismógrafo en formato "SISMO-XXX".
        /// Si no puede extraer el número, retorna el hash code.
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

        // ==========================================================
        // MÉTODO: ObtenerMotivosAsync
        // Recupera los tipos de motivo disponibles en la base.
        // ==========================================================
        public async Task<IEnumerable<MotivoTipo>> ObtenerMotivosAsync()
        {
            var motivos = await _motivoTipoRepo.ObtenerTodosAsync();
            return motivos ?? Enumerable.Empty<MotivoTipo>();
        }

        // ==========================================================
        // MÉTODO: buscarOrdenesDeInspeccion
        // Devuelve las órdenes cerrables para el empleado logueado.
        // ==========================================================
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

        // ==========================================================
        // MÉTODOS AUXILIARES PRIVADOS
        // ==========================================================

        private Estado BuscarEstadoCerradoParaOrdenInspeccion()
        {
            return _estadoRepo.ObtenerPorAmbitoYNombreAsync("OrdenInspeccion", "Cerrada").Result
                ?? throw new InvalidOperationException("Estado 'Cerrada' para 'OrdenInspeccion' no encontrado.");
        }

        private Estado BuscarEstadoSismografoFueraDeServicio()
        {
            return _estadoRepo.ObtenerPorAmbitoYNombreAsync("Sismografo", "FueraDeServicio").Result
                ?? throw new InvalidOperationException("Estado 'FueraDeServicio' para 'Sismografo' no encontrado.");
        }

        private List<string> ObtenerMailsResponsablesReparacion(IEnumerable<Empleado> empleados)
        {
            var mails = new List<string>();
            Console.WriteLine($"[DEBUG] Total empleados recibidos: {empleados.Count()}");
            
            // Loop: Recorrer Miembros [Mientras haya miembros]
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
    }
}
