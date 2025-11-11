using System.Collections.Generic;
using Aplicacion.Interfaces.Notificaciones;

namespace Aplicacion.Servicios.Notificaciones
{
    /// <summary>
    /// Implementación concreta del Sujeto dentro del patrón Observer.
    /// Responsable de gestionar la lista de observadores (correo, consola, etc.)
    /// y de notificarles cuando se produce un evento de cierre de orden.
    /// </summary>
    public class SujetoCierreOrden : ISujetoCierreOrden
    {
        // Lista interna de observadores suscritos
        private readonly List<IObservadorCierreOrden> _observadores;

        public SujetoCierreOrden()
        {
            _observadores = new List<IObservadorCierreOrden>();
        }

        /// <summary>
        /// Agrega un nuevo observador a la lista de suscriptores.
        /// </summary>
        /// <param name="observador">Observador concreto (ej. ObservadorEmailSMTP).</param>
        public void Suscribir(IObservadorCierreOrden observador)
        {
            if (observador != null && !_observadores.Contains(observador))
                _observadores.Add(observador);
        }

        /// <summary>
        /// Elimina un observador de la lista de suscriptores.
        /// </summary>
        /// <param name="observador">Observador a remover.</param>
        public void Desuscribir(IObservadorCierreOrden observador)
        {
            if (observador != null && _observadores.Contains(observador))
                _observadores.Remove(observador);
        }

        /// <summary>
        /// Notifica a todos los observadores suscritos.
        /// </summary>
        /// <param name="mensaje">Contenido textual de la notificación (ej. detalles de la orden).</param>
        /// <param name="destinatario">Dirección de correo del receptor principal.</param>
        public void Notificar(string mensaje, string destinatario)
        {
            foreach (var observador in _observadores)
            {
                try
                {
                    observador.Actualizar(mensaje, destinatario);
                }
                catch (System.Exception ex)
                {
                    // No interrumpe la notificación del resto en caso de error
                    System.Console.WriteLine($"[WARN] Error notificando observador: {ex.Message}");
                }
            }
        }
    }
}
