using Dominio.Entidades;
using System.Threading.Tasks;

namespace Dominio.Repositorios
{
    public interface IRepositorioEmpleado : IRepositorio<Empleado>
    {
        //Task<Empleado?> ObtenerPorMailAsync(string mail);
        Empleado ObtenerPorMail(string mail);


    }
}
