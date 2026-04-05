using FacturaElectronica.Infraestructura.Adapters.Inbound.Messaging.RabbitMq.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FacturaElectronica.Infraestructura.Adapters.Inbound.Messaging.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddFacturacionElectronicaMassTransit(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        
        var rabbitHost = configuration.GetSection("RabbitMqSettings:Uri").Value ?? Environment.GetEnvironmentVariable("RABBIT_HOST") ??
            throw new Exception("The rabbitMQ connection was not supled");
        
        services.AddMassTransit(x =>
        {
            // Registrar los consumers
            x.AddConsumer<FacturaElectronicaConsumer>();
            x.AddConsumer<NotaCreditoElectronicaConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitHost);

                // Configurar el endpoint para facturas electrónicas
                cfg.ReceiveEndpoint("factura-electronica-queue", e =>
                {
                    e.ConfigureConsumer<FacturaElectronicaConsumer>(context);
                });

                // Configurar el endpoint para notas de crédito electrónicas
                cfg.ReceiveEndpoint("nota-credito-electronica-queue", e =>
                {
                    e.ConfigureConsumer<NotaCreditoElectronicaConsumer>(context);
                });
            });
        });

        return services;
    }
}