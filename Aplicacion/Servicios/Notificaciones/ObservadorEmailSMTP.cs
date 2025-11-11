using Aplicacion.Interfaces.Notificaciones;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using System;
using System.Threading;

namespace Aplicacion.Servicios.Notificaciones
{
    public class ObservadorEmailSMTP : IObservadorCierreOrden
    {
        private readonly SmtpSettings _config;

        public ObservadorEmailSMTP(IOptions<SmtpSettings> config)
        {
            _config = config.Value;
        }

        public void Actualizar(string mensaje, string destinatario)
        {
            Console.WriteLine($"[DEBUG] Iniciando envío de email a {destinatario}...");
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_config.FromName, _config.FromAddress));
            email.To.Add(new MailboxAddress("", destinatario));
            email.Subject = "Notificación: Cierre de Orden de Inspección";
            email.Body = new TextPart("plain") { Text = mensaje };

            int intentos = 0;
            bool enviado = false;

            while (!enviado && intentos < 3)
            {
                try
                {
                    intentos++;
                    using var smtp = new SmtpClient();
                    Console.WriteLine($"[DEBUG] HOST={_config.Host}");
                    Console.WriteLine($"[DEBUG] USER={_config.User}");
                    Console.WriteLine($"[DEBUG] PASS={_config.Password}");


                    // ✅ usar StartTLS explícito, no Auto ni SslOnConnect
                    smtp.Connect(_config.Host, 587, SecureSocketOptions.StartTls);
                    Console.WriteLine("[SMTP] Conectado correctamente (StartTLS 587).");

                    smtp.Authenticate(_config.User, _config.Password);
                    smtp.Send(email);
                    smtp.Disconnect(true);

                    Console.WriteLine($"✅ [EMAIL] Notificación enviada correctamente a {destinatario} (intento {intentos}).");
                    enviado = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [EMAIL ERROR intento {intentos}] {ex.Message}");
                    if (intentos < 3)
                    {
                        Console.WriteLine("↻ Reintentando en 3 segundos...");
                        Thread.Sleep(3000);
                    }
                }
            }

            if (!enviado)
                Console.WriteLine("❌ [EMAIL] No se pudo enviar el correo después de 3 intentos.");
        }
    }
}
