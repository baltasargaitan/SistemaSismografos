using Dominio.Entidades;
using Infraestructura.Config;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Persistencia
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 🗂️ DbSets principales
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<MotivoTipo> MotivosTipo { get; set; }
        public DbSet<MotivoFueraServicio> MotivosFueraServicio { get; set; }
        public DbSet<EstacionSismologica> EstacionesSismologicas { get; set; }
        public DbSet<Sismografo> Sismografos { get; set; }
        public DbSet<OrdenDeInspeccion> OrdenesDeInspeccion { get; set; }
        public DbSet<CambioEstado> CambiosEstado { get; set; }
        public DbSet<Sesion> Sesiones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔧 Aplica todas las configuraciones de manera explícita
            modelBuilder.ApplyConfiguration(new EmpleadoConfig());
            modelBuilder.ApplyConfiguration(new RolConfig());
            modelBuilder.ApplyConfiguration(new UsuarioConfig());
            modelBuilder.ApplyConfiguration(new EstadoConfig());
            modelBuilder.ApplyConfiguration(new MotivoTipoConfig());
            modelBuilder.ApplyConfiguration(new MotivoFueraServicioConfig());
            modelBuilder.ApplyConfiguration(new EstacionSismologicaConfig());
            modelBuilder.ApplyConfiguration(new SismografoConfig());
            modelBuilder.ApplyConfiguration(new OrdenDeInspeccionConfig());
            modelBuilder.ApplyConfiguration(new CambioEstadoConfig());
            modelBuilder.ApplyConfiguration(new SesionConfig());

            base.OnModelCreating(modelBuilder);
        }
    }
}
