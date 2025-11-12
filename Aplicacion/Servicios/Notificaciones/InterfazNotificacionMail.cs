using System;
using Aplicacion.Interfaces.Notificaciones;
using Microsoft.Extensions.Options;

namespace Aplicacion.Servicios.Notificaciones
{
    /// <summary>
    /// InterfazNotificacionMail - Observador Concreto del patrón Observer.
    /// Responsable de enviar notificaciones por correo electrónico cuando
    /// se cierra una orden de inspección.
    /// Implementa fielmente el diagrama UML proporcionado.
    /// </summary>
    public class InterfazNotificacionMail : IObserverNotificacionCierre
    {
        // Atributos según diagrama
        private string _cuerpoEmail = string.Empty;
        private readonly SmtpSettings _smtpSettings;

        public InterfazNotificacionMail(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings?.Value ?? new SmtpSettings();
            Console.WriteLine("📧 InterfazNotificacionMail inicializada.");
        }

        /// <summary>
        /// actualizar(...): void
        /// Método según diagrama para recibir notificación y preparar el envío de mail.
        /// </summary>
        public void Actualizar(
            int idSismografo,
            string nombreEstado,
            DateTime fechaHoraCierre,
            string[] motivos,
            string[] comentarios,
            string[] mailsResponsablesReparacion)
        {
            // Generar cuerpo del email según diagrama
            _cuerpoEmail = GenerarCuerpoEmail(
                idSismografo,
                nombreEstado,
                fechaHoraCierre,
                motivos,
                comentarios);

            // Enviar mail a cada responsable
            // Loop: Recorrer mailsResponsablesReparacion
            if (mailsResponsablesReparacion != null && mailsResponsablesReparacion.Length > 0)
            {
                foreach (var mail in mailsResponsablesReparacion)
                {
                    EnviarMail(mail, _cuerpoEmail);
                }
            }
        }

        /// <summary>
        /// enviarMail(mailResponsableReparacion: String, cuerpoEmail: String): void
        /// Método según diagrama para envío real del correo.
        /// </summary>
        private void EnviarMail(string mailResponsableReparacion, string cuerpoEmail)
        {
            try
            {
                // Validar configuración SMTP
                if (string.IsNullOrWhiteSpace(_smtpSettings.User) ||
                    string.IsNullOrWhiteSpace(_smtpSettings.Password))
                {
                    Console.WriteLine($"[InterfazNotificacionMail] ⚠️ Configuración SMTP incompleta. Email simulado a: {mailResponsableReparacion}");
                    Console.WriteLine($"Contenido:\n{cuerpoEmail}");
                    return;
                }

                // Crear y enviar el mensaje
                var mensaje = new MimeKit.MimeMessage();
                mensaje.From.Add(new MimeKit.MailboxAddress(
                    _smtpSettings.FromName,
                    _smtpSettings.FromAddress));
                mensaje.To.Add(new MimeKit.MailboxAddress("", mailResponsableReparacion));
                mensaje.Subject = "Notificación de Cierre de Orden de Inspección";

                var bodyBuilder = new MimeKit.BodyBuilder
                {
                    TextBody = cuerpoEmail
                };
                mensaje.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                // Usar SecureSocketOptions.StartTls para STARTTLS en puerto 587
                client.Connect(_smtpSettings.Host, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(_smtpSettings.User, _smtpSettings.Password);
                client.Send(mensaje);
                client.Disconnect(true);

                Console.WriteLine($"[InterfazNotificacionMail] ✅ Email enviado correctamente a: {mailResponsableReparacion}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InterfazNotificacionMail] ❌ Error enviando email: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera el cuerpo del email con la información del cierre.
        /// </summary>
        private string GenerarCuerpoEmail(
            int idSismografo,
            string nombreEstado,
            DateTime fechaHoraCierre,
            string[] motivos,
            string[] comentarios)
        {
            var motivosTexto = motivos != null && motivos.Length > 0
                ? string.Join(", ", motivos)
                : "Sin motivos especificados";

            var comentariosTexto = comentarios != null && comentarios.Length > 0
                ? string.Join(", ", comentarios)
                : "Sin comentarios adicionales";

            return $"Estimado/a Responsable de Reparación,\n\n" +
                   $"Se ha registrado el cierre de una orden de inspección con los siguientes detalles:\n\n" +
                   $"• Sismógrafo ID: {idSismografo}\n" +
                   $"• Nuevo Estado: {nombreEstado}\n" +
                   $"• Fecha y Hora de Cierre: {fechaHoraCierre:G}\n" +
                   $"• Motivos: {motivosTexto}\n" +
                   $"• Comentarios: {comentariosTexto}\n\n" +
                   $"Por favor, tome las acciones necesarias.\n\n" +
                   $"Atentamente,\n" +
                   $"Sistema de Gestión de Sismógrafos - SISGRAFOS";
        }

        /// <summary>
        /// new(): InterfazNotificacionMail
        /// Constructor público según diagrama.
        /// </summary>
        public static InterfazNotificacionMail Crear(IOptions<SmtpSettings> smtpSettings)
        {
            return new InterfazNotificacionMail(smtpSettings);
        }
    }
}
