using CreditSystem.Application.Behaviors;
using CreditSystem.Application.Job;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CreditSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
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

    public static IServiceCollection AddJobsConfiguration(this IServiceCollection services)
    {
        services.AddScoped<IInterestAccrualJob, InterestAccrualJob>();
        return services;
    }
}