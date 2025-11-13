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
        /// Suscribe múltiples observadores al sujeto de una sola vez.
        /// Recibe un array de observadores y los añade a la lista interna.
        /// </summary>
        void Suscribir(IObserverNotificacionCierre[] observadores);

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
