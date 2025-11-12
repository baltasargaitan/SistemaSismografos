using System;

namespace Aplicacion.Interfaces.Notificaciones
{
    /// <summary>
    /// Interfaz del Observador en el patrón Observer.
    /// Define el método que será invocado cuando el sujeto notifique cambios.
    /// </summary>
    public interface IObserverNotificacionCierre
    {
        /// <summary>
        /// Actualiza al observador con la información del cierre de orden.
        /// </summary>
        /// <param name="idSismografo">Identificador del sismógrafo afectado</param>
        /// <param name="nombreEstado">Nombre del nuevo estado del sismógrafo</param>
        /// <param name="fechaHoraCierre">Fecha y hora del cierre</param>
        /// <param name="motivos">Lista de motivos del cierre</param>
        /// <param name="comentarios">Lista de comentarios asociados</param>
        /// <param name="mailsResponsablesReparacion">Correos de los responsables de reparación</param>
        void Actualizar(
            int idSismografo,
            string nombreEstado,
            DateTime fechaHoraCierre,
            string[] motivos,
            string[] comentarios,
            string[] mailsResponsablesReparacion);
    }
}
