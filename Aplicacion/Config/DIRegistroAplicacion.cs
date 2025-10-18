using Microsoft.Extensions.DependencyInjection;
using Aplicacion.UseCases;
using Aplicacion.Interfaces;

namespace Aplicacion.Config
{
    public static class DIRegistroAplicacion
    {
        public static IServiceCollection RegistrarAplicacion(this IServiceCollection services)
        {
            // Use cases / servicios de aplicación
            services.AddScoped<CerrarOrdenUseCase>();

            // Interfaces de infraestructura que debe suministrar Api/Infraestructura
            services.AddScoped<IPantallaCierreInspeccion, IPantallaCierreInspeccion>(); // placeholder; registrar implementación real en capa de presentación o Api
            services.AddScoped<INotificador, INotificador>(); // placeholder; registrar implementación real en Api/Infraestructura
            services.AddScoped<IInicioSesionService, IInicioSesionService>(); // placeholder

            return services;
        }
    }
}
