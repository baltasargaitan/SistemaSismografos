using Dominio.Entidades;

namespace Dominio.Repositorios
{
    public interface IRepositorioEstado : IRepositorio<Estado>
    {
        Task<Estado?> ObtenerPorAmbitoYNombreAsync(string ambito, string nombre);
    }
}
