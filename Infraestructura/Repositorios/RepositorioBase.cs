using Dominio.Repositorios;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositorios
{
    public class RepositorioBase<T> : IRepositorio<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositorioBase(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> ObtenerPorIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<List<T>> ObtenerTodosAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task AgregarAsync(T entidad)
        {
            await _dbSet.AddAsync(entidad);
        }

        public virtual void Actualizar(T entidad)
        {
            _dbSet.Update(entidad);
        }

        public virtual void Eliminar(T entidad)
        {
            _dbSet.Remove(entidad);
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IEnumerable<T> ObtenerTodos()
        {
            return _dbSet.ToList();
        }
    }
}
