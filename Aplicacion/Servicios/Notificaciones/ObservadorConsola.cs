using Aplicacion.Interfaces.Notificaciones;

namespace Aplicacion.Servicios.Notificaciones
{
    public class ObservadorConsola : IObservadorCierreOrden
    {
        public void Actualizar(string mensaje)
        {
            Console.WriteLine($"[MONITOR] {mensaje}");
        }
    }
}
