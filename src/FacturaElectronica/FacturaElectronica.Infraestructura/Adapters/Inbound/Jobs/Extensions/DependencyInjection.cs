using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FacturaElectronica.Infraestructura.Adapters.Inbound.Jobs.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddJobsLayer(this IServiceCollection services) => services
        .AddJobsServicesLayers();

    private static IServiceCollection AddJobsServicesLayers(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(PoolingInvoiceJob));

            configure
                .AddJob<PoolingInvoiceJob>(opts => opts.WithIdentity(jobKey))
                .AddTrigger(
                    trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                        schedule => schedule.WithIntervalInMinutes(3).RepeatForever()));

            //configure.UseMicrosoftDependencyInjectionJobFactory();
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        return services;
    }
}