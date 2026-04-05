using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Web.Hacienda.Extensions;

/// <summary>
/// Extensiones para configuración en DI
/// </summary>
public static class ServicioHaciendaExtensions
{
    public static IServiceCollection AddServiciosHacienda(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar settings
        services.Configure<HaciendaSettings>(configuration.GetSection("Hacienda"));

        // Registrar HttpClient con Polly para resilencia
        services.AddHttpClient<IServicioDocumentosHacienda, ServicioDocumentosHacienda>()
            .ConfigureHttpClient((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<HaciendaSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSegundos);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        
        services.AddHttpClient<IServicioAutenticacionHacienda, ServicioAutenticacionHacienda>()
            .ConfigureHttpClient((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<HaciendaSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSegundos);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30));
    }
}