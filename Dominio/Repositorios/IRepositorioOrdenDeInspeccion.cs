using System.Collections.Generic;
using System.Threading.Tasks;
using Dominio.Entidades;

namespace Dominio.Repositorios
{
    public interface IRepositorioOrdenDeInspeccion
    {
        Task<OrdenDeInspeccion?> BuscarPorNroAsync(int nroOrden);

        Task<List<OrdenDeInspeccion>> ObtenerTodasAsync();

        void Actualizar(OrdenDeInspeccion orden);

        Task GuardarCambiosAsync();

        Estado? BuscarEstado(string ambito, string nombreEstado);
    }
}
