using Aplicacion.DTOs;
using Aplicacion.UseCases;
using Dominio.Repositorios;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
    }
}
