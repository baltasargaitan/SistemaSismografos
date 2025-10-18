using Aplicacion.Interfaces;
using Dominio.Entidades;

namespace Aplicacion.Servicios
{
    public class InicioSesionServiceEnMemoria : IInicioSesionService
    {
        public Usuario ObtenerUsuarioLogueado()
        {
            var empleado = new Empleado("Juan","Pérez", "juan.perez@empresa.com", "3584207322");
            return new Usuario("admin", "1234", empleado);
        }
    }
}
