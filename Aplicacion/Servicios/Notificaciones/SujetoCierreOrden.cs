using System.Collections.Generic;
using Aplicacion.Interfaces.Notificaciones;
namespace Aplicacion.Servicios.Notificaciones
{

    public class SujetoCierreOrden : ISujetoCierreOrden
    {
        private readonly List<IObservadorCierreOrden> _observadores = new();

        public void Suscribir(IObservadorCierreOrden observador) => _observadores.Add(observador);
        public void Quitar(IObservadorCierreOrden observador) => _observadores.Remove(observador);
        public void Notificar(string mensaje)
        {
            foreach (var obs in _observadores)
                obs.Actualizar(mensaje);
        }
    }
}
