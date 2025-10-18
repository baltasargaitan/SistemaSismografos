using Aplicacion.Interfaces;
using Aplicacion.UseCases;
using Aplicacion.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace Aplicacion
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Método de extensión para registrar los servicios y casos de uso de la capa Aplicación.
        /// </summary>
        public static IServiceCollection AddAplicacion(this IServiceCollection services)
        {
            // 🧠 Casos de uso
            services.AddScoped<CerrarOrdenUseCase>();

            // 💼 Servicios de aplicación
            services.AddScoped<IInicioSesionService, InicioSesionServiceEnMemoria>();
            services.AddScoped<IPantallaCierreInspeccion, ConsolaPantallaCierreInspeccion>();
            services.AddScoped<INotificador, NotificadorConsola>();

            return services;
        }
    }
}
