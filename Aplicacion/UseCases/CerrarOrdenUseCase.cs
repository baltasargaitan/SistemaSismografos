using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Repositorios;
using System;

namespace Aplicacion.UseCases
{
    public class CerrarOrdenUseCase
    {
        private readonly IRepositorioOrdenDeInspeccion _ordenRepo;
        private readonly IRepositorioEmpleado _empleadoRepo;
        private readonly IRepositorioSismografo _sismografoRepo;
        private readonly INotificador _notificador;
        private readonly IInicioSesionService _sesionService;
        private readonly IRepositorioEstado _estadoRepo;
        private readonly IRepositorioMotivoTipo _motivoTipoRepo;

        public CerrarOrdenUseCase(
            IRepositorioOrdenDeInspeccion ordenRepo,
            IRepositorioEmpleado empleadoRepo,
            IRepositorioSismografo sismografoRepo,
            INotificador notificador,
            IInicioSesionService sesionService,
            IRepositorioMotivoTipo motivoTipoRepo,
            IRepositorioEstado estadoRepo)
        {
            _ordenRepo = ordenRepo;
            _empleadoRepo = empleadoRepo;
            _sismografoRepo = sismografoRepo;
            _notificador = notificador;
            _sesionService = sesionService;
            _estadoRepo = estadoRepo;
            _motivoTipoRepo = motivoTipoRepo;
        }

        public async Task<string> CerrarOrdenAsync(CierreOrdenRequest request)
        {
            var usuario = _sesionService.ObtenerUsuarioLogueado();
            if (usuario == null)
                return "No hay usuario logueado.";

            var ordenEntidad = _ordenRepo.BuscarPorNro(request.NroOrden);
            if (ordenEntidad == null)
                return $"No se encontró la orden {request.NroOrden}.";

            if (string.IsNullOrWhiteSpace(request.Observacion))
                return "Debe ingresar una observación.";

            if (!request.Confirmar)
                return "Cierre cancelado por el usuario.";

            ordenEntidad.SetObservacion(request.Observacion);

            var motivoTipoSeleccionado = (await _motivoTipoRepo.ObtenerTodosAsync())
                .FirstOrDefault(m => m.TipoMotivo == request.MotivoTipo);

            if (motivoTipoSeleccionado == null)
                return "Motivo inválido.";

            var motivoObjeto = new MotivoFueraServicio(motivoTipoSeleccionado, request.Comentario);

            if (!ValidarDatosMinimosReqParaCierre(ordenEntidad))
                return "Datos mínimos requeridos incompletos.";

            var estadoCerrado = BuscarEstadoCerradoParaOrdenInspeccion();
            ordenEntidad.SetFechaHoraCierre();
            ordenEntidad.SetEstado(estadoCerrado);

            var estacion = ordenEntidad.GetEstacion();
            var sismografo = estacion?.ObtenerIdSismografo();
            if (sismografo != null)
            {
                var sismografoPersistido = await _sismografoRepo.ObtenerPorIdentificacionAsync(sismografo.GetIdentificadorSismografo());
                var estadoFueraServicio = BuscarEstadoSismografoFueraDeServicio();

                if (sismografoPersistido.ObtenerEstadoActual().EsFueraDeServicio() || estadoFueraServicio.EsFueraDeServicio())
                {
                    var cambio = sismografoPersistido.CrearCambioEstado(estadoFueraServicio);
                    cambio.SetFechaHoraFin();
                    sismografoPersistido.SetEstadoActual(estadoFueraServicio);
                    sismografoPersistido.EnviarAReparar();
                    cambio.CrearMotivosFueraDeServicio(motivoObjeto);
                    await _sismografoRepo.ActualizarAsync(sismografoPersistido);
                }
            }

            _ordenRepo.Actualizar(ordenEntidad);
            await _ordenRepo.GuardarCambiosAsync();

            var empleados = await (_empleadoRepo.ObtenerTodosAsync())?? new List<Empleado>();
            var mailResp = ObtenerMailResponsableDeReparacion(empleados);

            var mensajeMonitoreo = $"Orden {ordenEntidad.GetNroOrden()} cerrada. Observación: {request.Observacion}";
            _notificador.PublicarEnMonitores(mensajeMonitoreo);
            if (!string.IsNullOrWhiteSpace(mailResp))
                _notificador.EnviarMail(mailResp, $"Orden {ordenEntidad.GetNroOrden()} cerrada", mensajeMonitoreo);

            return $"Orden {ordenEntidad.GetNroOrden()} cerrada correctamente.";
        }

        private bool ValidarDatosMinimosReqParaCierre(OrdenDeInspeccion orden)
        {
            var obs = orden.GetObservacion();
            var estadoActual = orden.GetEstado();
            if (string.IsNullOrWhiteSpace(obs)) return false;
            if (estadoActual != null && estadoActual.EsCerrada()) return false;
            return true;
        }

        private Estado BuscarEstadoCerradoParaOrdenInspeccion()
        {
            return _estadoRepo.ObtenerPorAmbitoYNombreAsync("OrdenInspeccion", "Cerrada").Result
                ?? throw new InvalidOperationException("Estado 'Cerrada' para 'OrdenInspeccion' no encontrado.");
        }


        public async Task<IEnumerable<OrdenDeInspeccion>> buscarOrdenesDeInspeccion()
        {
            var usuario = _sesionService.ObtenerUsuarioLogueado();
            if (usuario == null)
                return Enumerable.Empty<OrdenDeInspeccion>();

            var ordenes = await _ordenRepo.ObtenerTodasAsync();

            // Solo las completamente realizadas
            var cerrables = ordenes
                .Where(o => o.EstaCompletamenteRealizada())
                .ToList();

            return cerrables;
        }

        private Estado BuscarEstadoSismografoFueraDeServicio()
        {
            return _estadoRepo.ObtenerPorAmbitoYNombreAsync("Sismografo", "FueraDeServicio").Result
                ?? throw new InvalidOperationException("Estado 'FueraDeServicio' para 'Sismografo' no encontrado.");
        }

        private string ObtenerMailResponsableDeReparacion(IEnumerable<Empleado> empleados)
        {
            foreach (var emp in empleados)
                if (emp.EsResponsableDeReparacion())
                    return emp.ObtenerMail();
            return null;
        }
    }
}
