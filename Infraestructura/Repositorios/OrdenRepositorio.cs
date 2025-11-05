using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dominio.Entidades;
using Dominio.Repositorios;
using Microsoft.EntityFrameworkCore;
using Infraestructura.Persistencia;

namespace Infraestructura.Repositorios
{
    public class OrdenRepositorio : RepositorioBase<OrdenDeInspeccion>, IRepositorioOrdenDeInspeccion
    {
        private readonly AppDbContext _db;

        public OrdenRepositorio(AppDbContext context) : base(context)
        {
            _db = context;
        }

        public async Task<OrdenDeInspeccion?> BuscarPorNroAsync(int nroOrden)
        {
            return await _db.OrdenesDeInspeccion
                .Include(o => o.Estado)
                .Include(o => o.Estacion)
                .ThenInclude(e => e.Sismografos)
                .FirstOrDefaultAsync(o => o.NroOrden == nroOrden);
        }

        public async Task<List<OrdenDeInspeccion>> ObtenerTodasAsync()
        {
            return await _db.OrdenesDeInspeccion
                .Include(o => o.Estado)
                .Include(o => o.Estacion)
                .ThenInclude(e => e.Sismografos)
                .ToListAsync();
        }

        public Estado? BuscarEstado(string ambito, string nombreEstado)
        {
            return _db.Estados.FirstOrDefault(e => e.Ambito == ambito && e.NombreEstado == nombreEstado);
        }

        public void Actualizar(OrdenDeInspeccion orden)
        {
            _db.OrdenesDeInspeccion.Update(orden);
        }

        public async Task GuardarCambiosAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
