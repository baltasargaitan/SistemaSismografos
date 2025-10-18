using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using System;
using System.Collections.Generic;

namespace Aplicacion.Servicios
{
    public class ConsolaPantallaCierreInspeccion : IPantallaCierreInspeccion
    {
        public void HabilitarVentana() => Console.WriteLine("Ventana habilitada para cierre de orden.");
        public void MostrarMensaje(string mensaje) => Console.WriteLine($"[MENSAJE]: {mensaje}");
        public void MostrarSeleccionOrdenes(IEnumerable<OrdenResumenDTO> ordenes)
        {
            Console.WriteLine("Órdenes disponibles:");
            foreach (var o in ordenes)
                Console.WriteLine($" - {o.NroOrden} ({o.EstacionNombre}) [{o.Estado}]");
        }
        public int TomarSeleccionOrden()
        {
            Console.Write("Ingrese número de orden: ");
            int.TryParse(Console.ReadLine(), out int nro);
            return nro;
        }
        public string PedirObservacion()
        {
            Console.Write("Ingrese observación: ");
            return Console.ReadLine() ?? string.Empty;
        }
        public void MostrarMotivosFueraDeServicioSeleccionables(IEnumerable<string> motivos)
        {
            Console.WriteLine("Motivos disponibles:");
            foreach (var m in motivos)
                Console.WriteLine($" - {m}");
        }
        public string TomarSeleccionMotivoFueraDeServicio()
        {
            Console.Write("Seleccione motivo: ");
            return Console.ReadLine() ?? string.Empty;
        }
        public string SolicitarIngresoComentario()
        {
            Console.Write("Comentario adicional: ");
            return Console.ReadLine() ?? string.Empty;
        }
        public bool SolicitarConfirmacionCierreOrden()
        {
            Console.Write("¿Confirmar cierre? (S/N): ");
            var resp = Console.ReadLine()?.Trim().ToUpper();
            return resp == "S" || resp == "SI";
        }
    }
}
