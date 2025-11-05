using Aplicacion.Interfaces.Notificaciones;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;


namespace Aplicacion.Servicios.Notificaciones
{
    public class ObservadorEmailSMTP : IObservadorCierreOrden
    {
        private readonly SmtpSettings _config;

        public ObservadorEmailSMTP(IOptions<SmtpSettings> config)
        {
            _config = config.Value;
        }

        public void Actualizar(string mensaje)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_config.FromName, _config.FromAddress));
            email.To.Add(new MailboxAddress("Desde: ", _config.FromAddress)); 
            email.Subject = "Notificación: Cierre de Orden de Inspección";
            email.Body = new TextPart("plain") { Text = mensaje };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.Host, _config.Port, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.User, _config.Password);
            smtp.Send(email);
            smtp.Disconnect(true);

            Console.WriteLine("[EMAIL] Notificación enviada correctamente.");
        }
    }
}
