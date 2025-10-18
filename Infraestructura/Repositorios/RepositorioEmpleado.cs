using System.Linq;
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

        public Empleado ObtenerPorMail(string mail)
        {
            if (string.IsNullOrWhiteSpace(mail)) return null;
            return _db.Empleados.FirstOrDefault(e => e.Mail == mail);
        }
    }
}
