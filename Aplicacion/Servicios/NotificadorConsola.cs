using Aplicacion.Interfaces;
using System;

namespace Aplicacion.Servicios
{
    public class NotificadorConsola : INotificador
    {
        public void EnviarMail(string destino, string asunto, string cuerpo)
        {
            Console.WriteLine($"[MAIL a {destino}] {asunto}\n{cuerpo}");
        }

        public void PublicarEnMonitores(string mensaje)
        {
            Console.WriteLine($"[MONITOR]: {mensaje}");
        }
    }
}
