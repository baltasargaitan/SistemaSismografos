using System.Collections.Generic;
using System.Threading.Tasks;
using Dominio.Entidades;

namespace Dominio.Repositorios
{
    public interface IRepositorioMotivoTipo
    {
        Task<List<MotivoTipo>> ObtenerTodosAsync();
        Task<MotivoTipo> ObtenerPorTipoMotivoAsync(string tipoMotivo);
    }
}
