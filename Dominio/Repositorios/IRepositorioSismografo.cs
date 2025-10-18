// Dominio/Repositorios/IRepositorioSismografo.cs
using Dominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominio.Repositorios
{
    public interface IRepositorioSismografo
    {
        Task<Sismografo> ObtenerPorIdentificacionAsync(string identificacion);
        Task<IEnumerable<Sismografo>> ObtenerTodosAsync();
        Task ActualizarAsync(Sismografo sismografo);
        Task GuardarCambiosAsync();
    }
}
