using Dominio.Repositorios;
using Infraestructura.Persistencia;
using Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infraestructura
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddScoped<IRepositorioOrdenDeInspeccion, OrdenRepositorio>();
            services.AddScoped<IRepositorioEmpleado, EmpleadoRepositorio>();
            services.AddScoped<IRepositorioEstado, RepositorioEstado>();
            services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
            services.AddScoped<IRepositorioMotivoTipo, RepositorioMotivoTipo>();
            services.AddScoped<IRepositorioSismografo, RepositorioSismografo>();
            services.AddScoped<IRepositorioMotivoFueraServicio, RepositorioMotivoFueraServicio>();

            services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioBase<>));

            return services;
        }
    }
}
