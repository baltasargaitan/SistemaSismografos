using Dominio.Entidades;

namespace Dominio.Repositorios
{
    public interface IRepositorioEstacionSismologica : IRepositorio<EstacionSismologica>
    {
        // Podés agregar métodos específicos, por ejemplo:
        Task<EstacionSismologica?> ObtenerPorCodigoAsync(string codigo);
    }
}
