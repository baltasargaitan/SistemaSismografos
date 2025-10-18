using Dominio.Entidades;
using System.Threading.Tasks;

namespace Dominio.Repositorios
{
    public interface IRepositorioUsuario : IRepositorio<Usuario>
    {
        Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario);
    }
}
