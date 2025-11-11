using System;
using Aplicacion.Interfaces.Notificaciones;

namespace Aplicacion.Servicios.Notificaciones
{
    /// <summary>
    /// Observador concreto del patrón Observer.
    /// Su función es registrar en consola los eventos de cierre de orden,
    /// principalmente para propósitos de monitoreo y diagnóstico.
    /// </summary>
    public class ObservadorConsola : IObservadorCierreOrden
    {
        /// <summary>
        /// Método invocado por el sujeto cuando ocurre un evento de cierre.
        /// </summary>
        /// <param name="mensaje">Mensaje descriptivo del cierre.</param>
        /// <param name="destinatario">Correo del responsable o receptor del aviso.</param>
        public void Actualizar(string mensaje, string destinatario)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[MONITOR] Notificación emitida para: {destinatario}");
            Console.WriteLine($"[MONITOR] Detalle: {mensaje}");
            Console.WriteLine("==================================================");
        }
    }
}
