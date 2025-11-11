using Aplicacion.DTOs;
using Aplicacion.UseCases;
using Dominio.Repositorios;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using Aplicacion.Servicios.Notificaciones; // 👈 agregado para acceder al ObservadorWebMonitor

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CierreOrdenController : ControllerBase
    {
        private readonly CerrarOrdenUseCase _useCase;

        public CierreOrdenController(CerrarOrdenUseCase useCase)
        {
            _useCase = useCase;
        }

        // ==========================================================
        // 1️⃣ ÓRDENES CERRABLES
        // ==========================================================
        [HttpGet("cerrables")]
        public async Task<IActionResult> ObtenerOrdenesCerrables()
        {
            var ordenes = await _useCase.buscarOrdenesDeInspeccion();

            var resultado = ordenes.Select(o => new
            {
                nroOrden = o.GetNroOrden(),
                estacion = o.GetEstacion()?.GetNombre(),
                estado = o.GetEstado()?.NombreEstado,
                fechaInicio = o.GetFechaHoraInicio()
            });

            return Ok(resultado);
        }

        // ==========================================================
        // 2️⃣ MOTIVOS DISPONIBLES
        // ==========================================================
        [HttpGet("motivos")]
        public async Task<IActionResult> ObtenerMotivos()
        {
            try
            {
                var motivos = await _useCase.ObtenerMotivosAsync();
                var resultado = motivos.Select(m => new
                {
                    tipoMotivo = m.TipoMotivo,
                    descripcion = m.Descripcion
                });
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener motivos: {ex.Message}");
            }
        }

        // ==========================================================
        // 3️⃣ CERRAR ORDEN
        // ==========================================================
        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarOrden([FromBody] CierreOrdenRequest request)
        {
            try
            {
                var resultado = await _useCase.CerrarOrdenAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cerrar la orden: {ex.Message}");
            }
        }

        // ==========================================================
        // 4️⃣ MONITOREO DE EVENTOS (Observer)
        // ==========================================================
        [HttpGet("monitoreo")]
        public IActionResult ObtenerEventosDeMonitoreo()
        {
            try
            {
                var eventos = ObservadorWebMonitor.ObtenerEventos();
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener eventos de monitoreo: {ex.Message}");
            }
        }
    }
}
