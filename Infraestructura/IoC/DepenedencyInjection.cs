using Dominio.Repositorios;
using Infraestructura.Persistencia;
using Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infraestructura.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            // Repositorios
            services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioBase<>));
            services.AddScoped<IRepositorioOrdenDeInspeccion, OrdenRepositorio>();
            services.AddScoped<IRepositorioEstacionSismologica, RepositorioEstacionSismologica>();
            services.AddScoped<IRepositorioEmpleado, EmpleadoRepositorio>();
            services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
            services.AddScoped<IRepositorioEstado, RepositorioEstado>();
            services.AddScoped<IRepositorioMotivoTipo, RepositorioMotivoTipo>();
            services.AddScoped<IRepositorioMotivoFueraServicio, RepositorioMotivoFueraServicio>();

            return services;
        }
    }
}
