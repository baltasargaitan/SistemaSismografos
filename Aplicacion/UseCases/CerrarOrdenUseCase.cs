using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Aplicacion.Interfaces.Notificaciones;
using Dominio.Entidades;
using Dominio.Repositorios;

namespace Aplicacion.UseCases
{
    public class CerrarOrdenUseCase
    {
        private readonly IRepositorioOrdenDeInspeccion _ordenRepo;
        private readonly IRepositorioEmpleado _empleadoRepo;
        private readonly IRepositorioSismografo _sismografoRepo;
        private readonly IInicioSesionService _sesionService;
        private readonly IRepositorioEstado _estadoRepo;
        private readonly IRepositorioMotivoTipo _motivoTipoRepo;
        private readonly ISujetoCierreOrden _sujeto;

    public CerrarOrdenUseCase(
        IRepositorioOrdenDeInspeccion ordenRepo,
        IRepositorioEmpleado empleadoRepo,
        IRepositorioSismografo sismografoRepo,
        IInicioSesionService sesionService,
        IRepositorioMotivoTipo motivoTipoRepo,
        IRepositorioEstado estadoRepo,
        ISujetoCierreOrden sujeto)
        {
            _ordenRepo = ordenRepo;
            _empleadoRepo = empleadoRepo;
            _sismografoRepo = sismografoRepo;
            _sesionService = sesionService;
            _estadoRepo = estadoRepo;
            _motivoTipoRepo = motivoTipoRepo;
            _sujeto = sujeto;
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
        // MÉTODO: CerrarOrdenAsync
        // Implementa el caso de uso "Dar cierre a orden de inspección".
        // Cumple la secuencia del diagrama UML sin alterar la interfaz
        // pública de las entidades.
        // ==========================================================
        public async Task<string> CerrarOrdenAsync(CierreOrdenRequest request)
        {
            // 1. Validación del usuario en sesión
            var usuario = _sesionService.ObtenerUsuarioLogueado();
            if (usuario == null)
                return "No hay usuario logueado.";

            // 2. Buscar orden por número
            var ordenEntidad = await _ordenRepo.BuscarPorNroAsync(request.NroOrden);
            if (ordenEntidad == null)
                return $"No se encontró la orden {request.NroOrden}.";

            // 3. Validaciones básicas de entrada
            if (string.IsNullOrWhiteSpace(request.Observacion))
                return "Debe ingresar una observación.";

            if (!request.Confirmar)
                return "Cierre cancelado por el usuario.";

            // 4. Validar estado de la orden
            if (ordenEntidad.GetEstado()?.EsCerrada() == true)
                return "La orden ya está cerrada.";

            var estadoCerrado = BuscarEstadoCerradoParaOrdenInspeccion();

            // 5. Validar motivos seleccionados
            var motivosTiposRepo = await _motivoTipoRepo.ObtenerTodosAsync();
            if (request.MotivosTipo == null || request.MotivosTipo.Count == 0)
                return "Debe seleccionar al menos un motivo.";

            // 6. Cerrar la orden según el método de dominio
            try
            {
                ordenEntidad.Cerrar(request.Observacion, estadoCerrado);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            // =====================================================
            // BLOQUE: Actualización del Sismógrafo
            // (Secuencia según el diagrama UML)
            // =====================================================
            var estacion = ordenEntidad.GetEstacion();
            var sismografo = estacion?.ObtenerIdSismografo();

            if (sismografo != null)
            {
                // Obtener el sismógrafo persistido por su identificador
                var sismografoPersistido = await _sismografoRepo.ObtenerPorIdentificacionAsync(
                    sismografo.GetIdentificadorSismografo()
                );

                // Validar la relación según la secuencia
                if (sismografoPersistido.SosDeEstacionSismologica(estacion))
                {
                    var estadoFueraServicio = BuscarEstadoSismografoFueraDeServicio();

                    // Crear el nuevo cambio de estado
                    var cambio = sismografoPersistido.CrearCambioEstado(estadoFueraServicio);

                    // Registrar fin del estado anterior
                    cambio.SetFechaHoraFin();

                    // Actualizar estado actual y marcar envío a reparación
                    sismografoPersistido.SetEstadoActual(estadoFueraServicio);
                    sismografoPersistido.EnviarAReparar();

                    // Crear y asociar motivos fuera de servicio
                    foreach (var tipo in request.MotivosTipo)
                    {
                        var tipoEncontrado = motivosTiposRepo.FirstOrDefault(
                            m => m.TipoMotivo == tipo || m.Descripcion == tipo
                        );

                        if (tipoEncontrado != null)
                        {
                            var comentario = request.Comentarios.ElementAtOrDefault(
                                request.MotivosTipo.IndexOf(tipo)
                            ) ?? string.Empty;

                            var motivo = new MotivoFueraServicio(tipoEncontrado, comentario);
                            cambio.CrearMotivosFueraDeServicio(motivo);
                        }
                    }

                    // Persistir el sismógrafo modificado
                    await _sismografoRepo.ActualizarAsync(sismografoPersistido);
                }
            }

            // =====================================================
            // BLOQUE: Persistencia de la orden cerrada
            // =====================================================
            _ordenRepo.Actualizar(ordenEntidad);
            await _ordenRepo.GuardarCambiosAsync();

            // =====================================================
            // BLOQUE: Notificación de cierre (Observer)
            // =====================================================
            var empleados = await (_empleadoRepo.ObtenerTodosAsync()) ?? new List<Empleado>();
            var mailResp = ObtenerMailResponsableDeReparacion(empleados);

            var empleadoRI = usuario.GetRILogueado();
            var mensaje =
                $"Orden {ordenEntidad.GetNroOrden()} cerrada por {empleadoRI.GetNombreCompleto()} ({empleadoRI.ObtenerMail()}).\n" +
                $"Observación: {request.Observacion}\nFecha de cierre: {DateTime.Now:G}.";

            // Se notifica a todos los observadores registrados (monitor, correo, etc.)
            _sujeto.Notificar(mensaje, mailResp ?? empleadoRI.ObtenerMail());

            return $"Orden {ordenEntidad.GetNroOrden()} cerrada correctamente. Notificación enviada.";
        }

        // ==========================================================
        // MÉTODO: buscarOrdenesDeInspeccion
        // Devuelve las órdenes realizables para el empleado logueado.
        // ==========================================================
        public async Task<IEnumerable<OrdenDeInspeccion>> buscarOrdenesDeInspeccion()
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

            // Log de diagnóstico en consola
            Console.WriteLine($"Empleado logueado: {empleado.ObtenerMail()}");
            Console.WriteLine($"Cantidad total de órdenes: {ordenes.Count()}");

            foreach (var o in ordenes)
                Console.WriteLine($"Orden {o.GetNroOrden()} pertenece={o.EsDeEmpleado(empleado)} completada={o.EstaCompletamenteRealizada()}");

            return cerrables;
        }

        // ==========================================================
        // MÉTODOS AUXILIARES
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

        private string? ObtenerMailResponsableDeReparacion(IEnumerable<Empleado> empleados)
        {
            foreach (var emp in empleados)
                if (emp.EsResponsableDeReparacion())
                    return emp.ObtenerMail();
            return null;
        }
    }
}
