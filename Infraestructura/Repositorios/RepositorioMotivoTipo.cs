using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Repositorios;
using Infraestructura.Persistencia; // Para usar AppDbContext

namespace Infraestructura.Repositorios
{
    public class RepositorioMotivoTipo : IRepositorioMotivoTipo
    {
        private readonly AppDbContext _context;

        public RepositorioMotivoTipo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MotivoTipo>> ObtenerTodosAsync()
        {
            return await _context.MotivosTipo.ToListAsync();
        }

        public async Task<MotivoTipo> ObtenerPorTipoMotivoAsync(string tipoMotivo)
        {
            return await _context.MotivosTipo.FirstOrDefaultAsync(m => m.TipoMotivo == tipoMotivo);
        }
    }
}
