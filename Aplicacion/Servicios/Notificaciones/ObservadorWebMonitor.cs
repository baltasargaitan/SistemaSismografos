using Aplicacion.Interfaces.Notificaciones;
using System.Collections.Concurrent;

namespace Aplicacion.Servicios.Notificaciones
{
    public class ObservadorWebMonitor : IObservadorCierreOrden
    {
        // Cola concurrente para almacenar los últimos eventos notificados
        private static readonly ConcurrentQueue<string> _eventos = new ConcurrentQueue<string>();

        public void Actualizar(string mensaje, string destinatario)
        {
            var texto = $"[{DateTime.Now:HH:mm:ss}] Notificación para {destinatario}: {mensaje}";
            _eventos.Enqueue(texto);

            // Limitamos a 100 eventos en memoria
            while (_eventos.Count > 100)
                _eventos.TryDequeue(out _);
        }

        // Método auxiliar para exponer los eventos al frontend
        public static IEnumerable<string> ObtenerEventos()
        {
            return _eventos.ToArray();
        }
    }
}
