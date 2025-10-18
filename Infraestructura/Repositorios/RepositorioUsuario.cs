using Dominio.Entidades;
using Dominio.Repositorios;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositorios
{
    public class RepositorioUsuario : RepositorioBase<Usuario>, IRepositorioUsuario
    {
        public RepositorioUsuario(AppDbContext context) : base(context) { }

        public async Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
        }
    }
}
