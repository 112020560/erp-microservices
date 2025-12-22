using CreditSystem.Domain.Abstractions;
using CreditSystem.Domain.Abstractions.Projections;
using CreditSystem.Domain.Abstractions.Services;
using CreditSystem.Infrastructure.EventStore;
using CreditSystem.Infrastructure.Messaging.RabbitMq.Consumers;
using CreditSystem.Infrastructure.Projections;
using CreditSystem.Infrastructure.Projectors;
using CreditSystem.Infrastructure.Repositories;
using CreditSystem.Infrastructure.Services;
using CreditSystem.Infrastructure.Workers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CreditSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddEventStore(configuration)
            .AddProjectorStore(configuration)
            .AddConsumerConfiguration(configuration)
            .AddPersistenseService(configuration)
            .AddWorkerConfiguration(configuration);
    
    private static IServiceCollection AddEventStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEventSerializer, JsonEventSerializer>();
        services.AddSingleton<IHashGenerator, Sha256HashGenerator>();
        services.AddScoped<IEventStore>(sp => 
            new PostgresEventStore(
                configuration.GetConnectionString("EventStore")!,
                sp.GetRequiredService<IEventSerializer>(),
                sp.GetRequiredService<IHashGenerator>(),
                sp.GetRequiredService<ILogger<PostgresEventStore>>()));
        services.AddScoped<ILoanContractRepository, LoanContractRepository>();
        
        
        return services;
    }

    private static IServiceCollection AddProjectorStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IProjectionStore>(sp =>
            new PostgresProjectionStore(
                configuration.GetConnectionString("ReadDb")!,
                sp.GetRequiredService<ILogger<PostgresProjectionStore>>()));
        
        services.AddScoped<IProjection, LoanSummaryProjector>();
        services.AddScoped<IProjection, DelinquentLoansProjector>();
        services.AddScoped<IProjection, PaymentHistoryProjector>();
        services.AddScoped<IProjection, LoanPortfolioProjector>();

        services.AddScoped<IProjectionEngine,ProjectionEngine>();
        
        return services;
    }

    private static IServiceCollection AddPersistenseService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILoanQueryService>(sp =>
            new LoanQueryService(configuration.GetConnectionString("CreditDb")!));
        return services;
    }

    private static IServiceCollection AddWorkerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<InterestAccrualWorker>();
        return services;
    }

    private static IServiceCollection AddConsumerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICustomerService>(sp =>
            new CustomerService(configuration.GetConnectionString("CreditDb")!));
        
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CustomerCreatedConsumer>();
            x.AddConsumer<CustomerUpdatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"]);

                cfg.ReceiveEndpoint("credit-service-customer-events", e =>
                {
                    e.ConfigureConsumer<CustomerCreatedConsumer>(context);
                    e.ConfigureConsumer<CustomerUpdatedConsumer>(context);
                });
            });
        });

        return services;
    }
}