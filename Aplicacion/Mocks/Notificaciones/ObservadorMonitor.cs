using System;
using Aplicacion.Interfaces.Notificaciones;

namespace Aplicacion.Mocks.Notificaciones
{
    public class ObservadorMonitor : IObservadorCierreOrden
    {
        public void Actualizar(string mensaje)
        {
            Console.WriteLine($"[MONITOR] {mensaje}");
        }
    }
}
