using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominio.Repositorios
{
    public interface IRepositorio<T> where T : class
    {
        Task<T?> ObtenerPorIdAsync(int id);
        Task<List<T>> ObtenerTodosAsync();
        IEnumerable<T> ObtenerTodos();
        Task AgregarAsync(T entidad);
        void Actualizar(T entidad);
        void Eliminar(T entidad);
        Task GuardarCambiosAsync();
    }
}
