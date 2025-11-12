using Aplicacion.DTOs;
using Aplicacion.Servicios.Notificaciones;
using Dominio.Repositorios;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CierreOrdenController : ControllerBase
    {
        private readonly GestorCierreInspeccion _gestor;

        public CierreOrdenController(GestorCierreInspeccion gestor)
        {
            _gestor = gestor;
        }

        // ==========================================================
        // 1️⃣ ÓRDENES CERRABLES
        // ==========================================================
        [HttpGet("cerrables")]
        public async Task<IActionResult> ObtenerOrdenesCerrables()
        {
            var ordenes = await _gestor.BuscarOrdenesDeInspeccion();

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
                var motivos = await _gestor.ObtenerMotivosAsync();
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
                var resultado = await _gestor.CerrarOrdenInspeccion(request);
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
