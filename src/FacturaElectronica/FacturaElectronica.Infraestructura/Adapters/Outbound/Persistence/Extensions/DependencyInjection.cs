using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Abstracciones.Services;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;
using FacturaElectronica.Infraestructura.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .ConfigurePersistenceService(configuration)
            .AddHealthChecks(configuration);

    private static IServiceCollection ConfigurePersistenceService(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("electronic_invoice_db") ??
                               throw new Exception("Connection string 'electronic_invoice_db' not found");

        // DbContext for FacturaElectronica (PostgreSQL)
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Warning)
                .EnableDetailedErrors()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );

        // Repositories
        services.AddScoped<IElectronicDocumentLogRepository, ElectronicDocumentLogRepository>();
        services.AddScoped<IElectronicInvoiceRepository, ElectronicInvoiceRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantCertificateRepository, TenantCertificateRepository>();
        services.AddScoped<ITenantHaciendaConfigRepository, TenantHaciendaConfigRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Certificate and encryption services
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddScoped<ICertificadoProvider, CertificadoProvider>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("electronic_invoice_db")!);

        return services;
    }
}
