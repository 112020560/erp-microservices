using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Infrastructure.Messaging;
using Catalogs.Infrastructure.Persistence;
using Catalogs.Infrastructure.Persistence.Outbox;
using Catalogs.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalogs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddPersistence(configuration)
            .AddMessaging(configuration);

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("CatalogsDb")
            ?? throw new InvalidOperationException("Connection string 'CatalogsDb' is not configured.");

        services.AddSingleton<DomainEventsInterceptor>();

        services.AddDbContext<CatalogsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((_, cfg) =>
            {
                cfg.Host(configuration["RabbitMqSettings:Uri"]);
            });
        });

        services.AddHostedService<OutboxDeliveryWorker>();

        return services;
    }
}
