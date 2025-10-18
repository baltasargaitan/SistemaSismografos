namespace Aplicacion.Interfaces
{
    public interface INotificador
    {
        void EnviarMail(string mailDestino, string asunto, string cuerpo);
        void PublicarEnMonitores(string mensaje);
    }
}
