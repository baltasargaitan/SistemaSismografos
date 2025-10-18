using Aplicacion.Interfaces;
using Dominio.Entidades;

namespace Aplicacion.Mocks
{
    public class MockInicioSesionService : IInicioSesionService
    {
        public Usuario ObtenerUsuarioLogueado()
        {
            return new Usuario("admin", "1234");
        }
    }
}
