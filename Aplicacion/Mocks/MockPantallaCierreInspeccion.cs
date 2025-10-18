using Aplicacion.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.Mocks
{
    public class MockPantallaCierreInspeccion : IPantallaCierreInspeccion
    {
        public void HabilitarVentana() => Console.WriteLine("Ventana habilitada para cierre de orden.");
        public void MostrarMensaje(string mensaje) => Console.WriteLine($"[MENSAJE]: {mensaje}");
        public void MostrarSeleccionOrdenes(IEnumerable<OrdenResumenDTO> ordenes)
        {
            Console.WriteLine("Órdenes pendientes:");
            foreach (var o in ordenes)
                Console.WriteLine($"- #{o.NroOrden} ({o.EstacionNombre}) [{o.Estado}]");
        }

        public int TomarSeleccionOrden() => 1;
        public string PedirObservacion() => "Observación de prueba";
        public void MostrarMotivosFueraDeServicioSeleccionables(IEnumerable<string> motivos)
        {
            Console.WriteLine("Motivos disponibles: " + string.Join(", ", motivos));
        }
        public string TomarSeleccionMotivoFueraDeServicio() => "Falla Sensor";
        public string SolicitarIngresoComentario() => "Comentario técnico";
        public bool SolicitarConfirmacionCierreOrden() => true;
    }
}
