using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Repositorios;
using System.Linq;

namespace Aplicacion.Servicios
{
    public class InicioSesionServiceEnMemoria : IInicioSesionService
    {
        private readonly IRepositorioEmpleado _empleadoRepo;

        public InicioSesionServiceEnMemoria(IRepositorioEmpleado empleadoRepo)
        {
            _empleadoRepo = empleadoRepo;
        }

        public Usuario ObtenerUsuarioLogueado()
        {
            // Buscar el empleado con el mail específico
            var empleado = _empleadoRepo.ObtenerTodosAsync().Result
                .FirstOrDefault(e => e.ObtenerMail() == "inspector@ejemplo.com");

            if (empleado == null)
            {
                // Si no existe en la base, crear uno temporal en memoria
                empleado = new Empleado("Juan", "Pérez", "inspector@ejemplo.com", "3584207322");
            }

            // Retornar el usuario logueado asociado a ese empleado
            return new Usuario("admin", "1234", empleado);
        }
    }
}
