using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Aplicacion.Interfaces.Notificaciones;
using Aplicacion.Servicios.Notificaciones;
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
        private readonly ISujetoCierreOrden _sujeto; // Sujeto central del patrón Observer


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

        // ========= CONSULTA DE MOTIVOS =========
        public async Task<IEnumerable<MotivoTipo>> ObtenerMotivosAsync()
        {
            var motivos = await _motivoTipoRepo.ObtenerTodosAsync();
            return motivos ?? Enumerable.Empty<MotivoTipo>();
        }

        // ========= CIERRE DE ORDEN =========
        public async Task<string> CerrarOrdenAsync(CierreOrdenRequest request)
        {
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

            var estadoCerrado = BuscarEstadoCerradoParaOrdenInspeccion();

            // Construir motivos
            var motivosTiposRepo = await _motivoTipoRepo.ObtenerTodosAsync();
            var motivosObjetos = new List<MotivoFueraServicio>();

            if (request.MotivosTipo == null || request.MotivosTipo.Count == 0)
                return "Debe seleccionar al menos un motivo.";

            for (int i = 0; i < request.MotivosTipo.Count; i++)
            {
                var tipo = request.MotivosTipo[i];
                var comentario = i < request.Comentarios.Count ? request.Comentarios[i] : "";
                var tipoEncontrado = motivosTiposRepo.FirstOrDefault(m =>
                    m.TipoMotivo == tipo || m.Descripcion == tipo);

                if (tipoEncontrado != null)
                    motivosObjetos.Add(new MotivoFueraServicio(tipoEncontrado, comentario));
            }

            if (motivosObjetos.Count == 0)
                return "Ninguno de los motivos es válido.";

            try
            {
                ordenEntidad.Cerrar(request.Observacion, estadoCerrado);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            // Actualización del sismógrafo
            var estacion = ordenEntidad.GetEstacion();
            var sismografo = estacion?.ObtenerIdSismografo();
            if (sismografo != null)
            {
                var sismografoPersistido = await _sismografoRepo.ObtenerPorIdentificacionAsync(
                    sismografo.GetIdentificadorSismografo()
                );

                var estadoFueraServicio = BuscarEstadoSismografoFueraDeServicio();

                if (sismografoPersistido.ObtenerEstadoActual().EsFueraDeServicio() ||
                    estadoFueraServicio.EsFueraDeServicio())
                {
                    var cambio = sismografoPersistido.CrearCambioEstado(estadoFueraServicio);
                    cambio.SetFechaHoraFin();
                    sismografoPersistido.SetEstadoActual(estadoFueraServicio);
                    sismografoPersistido.EnviarAReparar();

                    foreach (var m in motivosObjetos)
                        cambio.CrearMotivosFueraDeServicio(m);

                    await _sismografoRepo.ActualizarAsync(sismografoPersistido);
                }
            }

            // Persistencia
            _ordenRepo.Actualizar(ordenEntidad);
            await _ordenRepo.GuardarCambiosAsync();

            // ========= NOTIFICACIÓN OBSERVER =========
            var empleados = await (_empleadoRepo.ObtenerTodosAsync()) ?? new List<Empleado>();
            var mailResp = ObtenerMailResponsableDeReparacion(empleados);

            var empleadoRI = usuario.GetRILogueado();
            var mensaje =
                $"Orden {ordenEntidad.GetNroOrden()} cerrada por {empleadoRI.GetNombreCompleto()} ({empleadoRI.ObtenerMail()}).\n" +
                $"Observación: {request.Observacion}\nFecha de cierre: {DateTime.Now:G}.";

            // Notificar observadores
            _sujeto.Notificar(mensaje);

            return $"Orden {ordenEntidad.GetNroOrden()} cerrada correctamente. Notificación enviada.";
        }

        // ========= CONSULTA DE ORDENES CERRABLES =========
        public async Task<IEnumerable<OrdenDeInspeccion>> buscarOrdenesDeInspeccion()
        {
            var usuario = _sesionService.ObtenerUsuarioLogueado();
            if (usuario == null)
                return Enumerable.Empty<OrdenDeInspeccion>();

            var empleado = usuario.GetRILogueado();
            var ordenes = await _ordenRepo.ObtenerTodasAsync();

            var cerrables = ordenes
                .Where(o => o.EsDeEmpleado(empleado) && o.EstaCompletamenteRealizada())
                .ToList();

            Console.WriteLine($"Empleado logueado: {empleado.ObtenerMail()}");
            Console.WriteLine($"Cantidad total ordenes: {ordenes.Count()}");

            foreach (var o in ordenes)
                Console.WriteLine($"Orden {o.GetNroOrden()} pertenece={o.EsDeEmpleado(empleado)} completada={o.EstaCompletamenteRealizada()}");

            return cerrables;
        }

        // ========= HELPERS =========
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