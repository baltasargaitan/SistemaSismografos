using System;
using Aplicacion.Interfaces.Notificaciones;

namespace Aplicacion.Mocks.Notificaciones
{
    public class ObservadorEmail : IObservadorCierreOrden
    {
        private readonly string _destinatario;

        public ObservadorEmail(string destinatario)
        {
            _destinatario = destinatario;
        }

        public void Actualizar(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(_destinatario))
                Console.WriteLine("[EMAIL] No se encontró destinatario Responsable de Reparación.");
            else
                Console.WriteLine($"[EMAIL] Enviando mail a {_destinatario}: {mensaje}");
        }
    }
}
