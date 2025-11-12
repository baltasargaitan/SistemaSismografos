using System;
using Aplicacion.Interfaces.Notificaciones;

namespace Aplicacion.Servicios.Notificaciones
{
    /// <summary>
    /// PantallaCCRS - Observador Concreto del patrón Observer.
    /// Representa la pantalla de Cierre de Orden (CCRS) que recibe notificaciones
    /// cuando se cierra una orden de inspección.
    /// Implementa fielmente el diagrama UML proporcionado.
    /// </summary>
    public class PantallaCCRS : IObserverNotificacionCierre
    {
        // Atributos según diagrama
        private string _cuerpoPublicacion = string.Empty;
        private int _idSismografo;
        private string _nombreEstado = string.Empty;
        private DateTime _fechaHoraCierre;
        private string[] _motivos = Array.Empty<string>();
        private string[] _comentarios = Array.Empty<string>();
        private string[] _mailsResponsables = Array.Empty<string>();

        public PantallaCCRS()
        {
            Console.WriteLine("📋 PantallaCCRS inicializada.");
        }

        /// <summary>
        /// actualizar(...): void
        /// Método según diagrama de clases para actualizar la pantalla con información del cierre.
        /// </summary>
        public void Actualizar(
            int idSismografo,
            string nombreEstado,
            DateTime fechaHoraCierre,
            string[] motivos,
            string[] comentarios,
            string[] mailsResponsablesReparacion)
        {
            // Actualizar atributos internos
            _idSismografo = idSismografo;
            _nombreEstado = nombreEstado;
            _fechaHoraCierre = fechaHoraCierre;
            _motivos = motivos;
            _comentarios = comentarios;
            _mailsResponsables = mailsResponsablesReparacion;

            // Llamar a métodos según diagrama
            SetIdSismografo(idSismografo);
            SetNombreEstado(nombreEstado);
            SetFechaHoraCierre(fechaHoraCierre);
            SetMotivos(motivos);
            SetComentarios(comentarios);

            // Generar y mostrar publicación
            _cuerpoPublicacion = GenerarCuerpoPublicacion();
            Console.WriteLine($"[PantallaCCRS] Publicación generada:\n{_cuerpoPublicacion}");

            // Registrar evento en el monitor web para consulta desde el frontend
            ObservadorWebMonitor.RegistrarEvento(_cuerpoPublicacion);
        }

        /// <summary>
        /// setIdSismografo(int): void
        /// </summary>
        private void SetIdSismografo(int id)
        {
            _idSismografo = id;
        }

        /// <summary>
        /// setNombreEstado(String): void
        /// </summary>
        private void SetNombreEstado(string estado)
        {
            _nombreEstado = estado;
        }

        /// <summary>
        /// setFechaHoraCierre(DateTime): void
        /// </summary>
        private void SetFechaHoraCierre(DateTime fecha)
        {
            _fechaHoraCierre = fecha;
        }

        /// <summary>
        /// setMotivos(String[]): void
        /// </summary>
        private void SetMotivos(string[] motivos)
        {
            _motivos = motivos ?? Array.Empty<string>();
        }

        /// <summary>
        /// setComentarios(String[]): void
        /// </summary>
        private void SetComentarios(string[] comentarios)
        {
            _comentarios = comentarios ?? Array.Empty<string>();
        }

        /// <summary>
        /// Genera el cuerpo de la publicación con la información del cierre.
        /// </summary>
        private string GenerarCuerpoPublicacion()
        {
            var motivosTexto = _motivos != null && _motivos.Length > 0
                ? string.Join(", ", _motivos)
                : "Sin motivos";

            var comentariosTexto = _comentarios != null && _comentarios.Length > 0
                ? string.Join(", ", _comentarios)
                : "Sin comentarios";

            var responsablesTexto = _mailsResponsables != null && _mailsResponsables.Length > 0
                ? string.Join(", ", _mailsResponsables)
                : "Sin responsables";

            return $"🔔 NOTIFICACIÓN DE CIERRE\n" +
                   $"━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                   $"Sismógrafo ID: {_idSismografo}\n" +
                   $"Nuevo Estado: {_nombreEstado}\n" +
                   $"Fecha de Cierre: {_fechaHoraCierre:G}\n" +
                   $"Motivos: {motivosTexto}\n" +
                   $"Comentarios: {comentariosTexto}\n" +
                   $"Responsables: {responsablesTexto}\n" +
                   $"━━━━━━━━━━━━━━━━━━━━━━━━━━";
        }

        /// <summary>
        /// new(): PantallaCCRS
        /// Constructor público según diagrama.
        /// </summary>
        public static PantallaCCRS Crear()
        {
            return new PantallaCCRS();
        }
    }
}
