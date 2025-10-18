using Aplicacion.Interfaces;

namespace Aplicacion.Mocks
{
    public class MockNotificador : INotificador
    {
        public void EnviarMail(string destino, string asunto, string mensaje)
        {
            Console.WriteLine($"📧 [Mail enviado] A: {destino} - Asunto: {asunto}");
        }

        public void PublicarEnMonitores(string mensaje)
        {
            Console.WriteLine($"📢 [Monitor] {mensaje}");
        }
    }
}
