using Microsoft.Extensions.DependencyInjection;
using Retail.Application.Pricing.Services;

namespace Retail.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<PriceListResolver>();

        return services;
    }
}
