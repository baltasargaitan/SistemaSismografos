namespace Aplicacion.Interfaces.Notificaciones
{
    /// <summary>
    /// Define el contrato para el sujeto del patrón Observer
    /// encargado de notificar el cierre de una orden de inspección.
    /// </summary>
    public interface ISujetoCierreOrden
    {
        void Suscribir(IObservadorCierreOrden observador);

        void Desuscribir(IObservadorCierreOrden observador);

        void Notificar(string mensaje, string destinatario);
    }
}
