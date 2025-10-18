using Dominio.Entidades;
using Dominio.Repositorios;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infraestructura.Persistencia
{
    public class RepositorioSismografo : IRepositorioSismografo
    {
        private readonly AppDbContext _context;

        public RepositorioSismografo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Sismografo> ObtenerPorIdentificacionAsync(string identificacion)
        {
            return await _context.Sismografos
                .Include(s => s.EstadoActual)
                .Include(s => s.CambiosEstado)
                .FirstOrDefaultAsync(s => s.IdentificacionSismografo == identificacion);
        }

        public async Task<IEnumerable<Sismografo>> ObtenerTodosAsync()
        {
            return await _context.Sismografos
                .Include(s => s.EstadoActual)
                .ToListAsync();
        }

        public async Task ActualizarAsync(Sismografo sismografo)
        {
            _context.Sismografos.Update(sismografo);
            await GuardarCambiosAsync();
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
