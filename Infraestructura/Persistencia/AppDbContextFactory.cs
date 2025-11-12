using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infraestructura.Persistencia
{
    // Esta clase solo se usa en tiempo de diseño (para 'dotnet ef')
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            //  Cadena de conexión directa — 
            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS;Database=SistemaSismografosDB;Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
