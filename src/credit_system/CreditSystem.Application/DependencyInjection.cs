using CreditSystem.Application.Behaviors;
using CreditSystem.Application.Configuration;
using CreditSystem.Application.Job;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CreditSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        => services.AddMediatRConfiguration()
                    .AddJobsConfiguration();
    

    private static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }

    private static IServiceCollection AddJobsConfiguration(this IServiceCollection services)
    {
        services.AddScoped<IInterestAccrualJob, InterestAccrualJob>();
        services.AddScoped<IPaymentMissedJob, PaymentMissedJob>();
        return services;
    }

    private static IServiceCollection AddConfigurationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LateFeeConfiguration>(
            configuration.GetSection("LateFee"));

        return services;
    }
}