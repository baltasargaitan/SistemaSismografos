using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Repositorios;
using Infraestructura.Persistencia;

namespace Infraestructura.Repositorios
{
    public class EmpleadoRepositorio : RepositorioBase<Empleado>, IRepositorioEmpleado
    {
        private readonly AppDbContext _db;

        public EmpleadoRepositorio(AppDbContext context) : base(context)
        {
            _db = context;
        }

        /// <summary>
        /// Sobrescribe el método base para incluir la navegación de Roles.
        /// Esto es necesario para que EsResponsableDeReparacion() funcione correctamente.
        /// </summary>
        public override async Task<List<Empleado>> ObtenerTodosAsync()
        {
            return await _db.Empleados
                .Include(e => e.Roles)  // 👈 Incluye la relación de Roles
                .ToListAsync();
        }

        public Empleado ObtenerPorMail(string mail)
        {
            if (string.IsNullOrWhiteSpace(mail)) return null;
            return _db.Empleados
                .Include(e => e.Roles)  // 👈 También incluir aquí
                .FirstOrDefault(e => e.Mail == mail);
        }
    }
}
