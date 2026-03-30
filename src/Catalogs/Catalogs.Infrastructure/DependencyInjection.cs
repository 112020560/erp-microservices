using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Infrastructure.Messaging;
using Catalogs.Infrastructure.Persistence;
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

        services.AddDbContext<CatalogsDbContext>(options =>
            options.UseNpgsql(connectionString));

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
        services.AddScoped<IEventPublisher, EventPublisher>();

        services.AddMassTransit(x =>
        {
            // MassTransit EF Core Outbox — persists messages atomically with domain changes
            x.AddEntityFrameworkOutbox<CatalogsDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(2);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMqSettings:Uri"]);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
