using Dominio.Entidades;
using Dominio.Repositorios;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositorios
{
    public class RepositorioEstacionSismologica : RepositorioBase<EstacionSismologica>, IRepositorioEstacionSismologica
    {
        public RepositorioEstacionSismologica(AppDbContext context) : base(context) { }

        public async Task<EstacionSismologica?> ObtenerPorCodigoAsync(string codigo)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.CodigoEstacion == codigo);
        }
    }
}
