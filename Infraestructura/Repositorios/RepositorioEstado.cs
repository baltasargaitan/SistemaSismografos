using Dominio.Entidades;
using Dominio.Repositorios;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositorios
{
    public class RepositorioEstado : RepositorioBase<Estado>, IRepositorioEstado
    {
        public RepositorioEstado(AppDbContext context) : base(context) { }

        public async Task<Estado?> ObtenerPorAmbitoYNombreAsync(string ambito, string nombre)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.Ambito == ambito && e.NombreEstado == nombre);
        }
    }
}
