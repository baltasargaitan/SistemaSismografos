namespace Aplicacion.Interfaces.Notificaciones
{
    public interface ISujetoCierreOrden
    {
        void Suscribir(IObservadorCierreOrden observador);
        void Quitar(IObservadorCierreOrden observador);
        void Notificar(string mensaje);
    }
}
