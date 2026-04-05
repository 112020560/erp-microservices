using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Retail.Domain.Pricing.Abstractions;
using Retail.Infrastructure.Persistence;
using Retail.Infrastructure.Persistence.Repositories;
using Retail.Infrastructure.Services;

namespace Retail.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddPersistence(configuration);

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("RetailDb")
            ?? throw new InvalidOperationException("Connection string 'RetailDb' is not configured.");

        services.AddDbContext<RetailDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPriceListRepository, PriceListRepository>();
        services.AddScoped<IChannelPriceListRepository, ChannelPriceListRepository>();
        services.AddScoped<ICustomerPriceListRepository, CustomerPriceListRepository>();
        services.AddScoped<ICustomerGroupRepository, CustomerGroupRepository>();
        services.AddScoped<IScheduledPriceChangeRepository, ScheduledPriceChangeRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<ScheduledPriceChangeApplierService>();

        return services;
    }
}
