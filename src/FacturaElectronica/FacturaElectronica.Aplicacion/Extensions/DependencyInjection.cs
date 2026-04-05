using System.Reflection;
using FacturaElectronica.Aplicacion.ProcesoFactura.Polling;
using FacturaElectronica.Aplicacion.Services;
using FacturaElectronica.Dominio.Abstracciones.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FacturaElectronica.Aplicacion.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services) => services
        .AddMediatRService()
        .AddServicesLayers();

    private static IServiceCollection AddMediatRService(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            //cfg.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            //cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        return services;
    }

    private static IServiceCollection AddServicesLayers(this IServiceCollection services)
    {
        services.AddScoped<IPollingFacturasService, PollingFacturasService>();
        services.AddScoped<ISendNotificationService, SendNotificationService>();
        return services;
    }
}