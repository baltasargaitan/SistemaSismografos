using System.Collections.Generic;

namespace Aplicacion.Interfaces.Notificaciones
{
    /// <summary>
    /// Interfaz del Sujeto en el patrón Observer.
    /// Define los métodos para gestionar observadores y notificarles cambios.
    /// </summary>
    public interface ISujetoOrdenInspeccion
    {
        /// <summary>
        /// Suscribe un observador al sujeto.
        /// </summary>
        void Suscribir(IObserverNotificacionCierre observador);

        /// <summary>
        /// Quita un observador del sujeto.
        /// </summary>
        void Quitar(IObserverNotificacionCierre observador);

        /// <summary>
        /// Notifica a todos los observadores suscritos.
        /// </summary>
        void Notificar();
    }
}
