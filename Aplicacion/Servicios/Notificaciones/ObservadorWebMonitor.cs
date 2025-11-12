using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Aplicacion.Servicios.Notificaciones
{
    /// <summary>
    /// ObservadorWebMonitor - Registro estático de eventos de cierre.
    /// Almacena notificaciones generadas por PantallaCCRS para que el frontend
    /// pueda consultarlas a través del endpoint /api/CierreOrden/monitoreo.
    /// NO es un observador del patrón Observer, solo una cola de eventos.
    /// </summary>
    public static class ObservadorWebMonitor
    {
        // Cola concurrente para almacenar los últimos eventos notificados
        private static readonly ConcurrentQueue<string> _eventos = new ConcurrentQueue<string>();

        /// <summary>
        /// Método estático para registrar eventos desde PantallaCCRS.
        /// Permite que PantallaCCRS registre sus notificaciones detalladas.
        /// </summary>
        public static void RegistrarEvento(string mensaje)
        {
            _eventos.Enqueue(mensaje);

            // Limitamos a 100 eventos en memoria
            while (_eventos.Count > 100)
                _eventos.TryDequeue(out _);

            Console.WriteLine($"[ObservadorWebMonitor] 📊 Evento registrado en cola para monitoreo web");
        }

        /// <summary>
        /// Método auxiliar para exponer los eventos al frontend.
        /// Usado por el endpoint GET /api/CierreOrden/monitoreo
        /// </summary>
        public static IEnumerable<string> ObtenerEventos()
        {
            return _eventos.ToArray();
        }
    }
}
