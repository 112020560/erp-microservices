using FacturaElectronica.Dominio.Servicios.Clave;
using FacturaElectronica.Dominio.Servicios.Consecutivo;
using FacturaElectronica.Dominio.Servicios.Documentos.Firmas;
using FacturaElectronica.Dominio.Servicios.Factory;
using FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;
using FacturaElectronica.Dominio.Servicios.Validaciones.Emisores;
using FacturaElectronica.Dominio.Servicios.Validaciones.Facturas;
using FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Detalles;
using FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Negocio;
using FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Resumen;
using FacturaElectronica.Dominio.Servicios.Validaciones.Receptores;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FacturaElectronica.Dominio.Extensions;

public static class BusinessValidationsExtension
{
    public static IServiceCollection ServicioDominio(this IServiceCollection services, IConfiguration configuration)
    => services.ConfiguracionServiciosDominio()
               .ConfigurarOptionPattern(configuration)
               .AddValidacionesFacturaElectronica();
    private static IServiceCollection ConfiguracionServiciosDominio(this IServiceCollection services)
    {
        services.AddScoped<IFirmaDocumentos, FirmaDocumentos>();
        services.AddScoped<IGeneradorConsecutivo, GeneradorConsecutivo>();
        services.AddScoped<IGeneradorClave, GeneradorClaveV44>();
        //services.AddScoped<IGeneradorDocumentos, GeneradorDocumentos>();

        // Registrar generadores de documentos
        services.AddScoped<GeneradorDocumentos>();
        services.AddScoped<GeneradorDocumentosV44>();
        services.AddScoped<GeneradorTiqueteV44>();       // Tiquete Electrónico v4.4
        services.AddScoped<GeneradorNotaCreditoV44>();   // Nota de Crédito v4.4
        services.AddScoped<GeneradorNotaDebitoV44>();    // Nota de Débito v4.4

        // Registrar factory
        services.AddScoped<IGeneradorDocumentosFactory, GeneradorDocumentosFactory>();

        return services;
    }

    private static IServiceCollection ConfigurarOptionPattern(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddOptions<ConfiguracionFacturaElectronica>()
        .Bind(configuration.GetSection("FacturaElectronica"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

        return services;
    }
    
    private static IServiceCollection AddValidacionesFacturaElectronica(this IServiceCollection services)
    {
        // Registrar validadores principales
        services.AddScoped<ValidadorFacturaV44>();
        services.AddScoped<ValidadorNegocioV44>();

        // Registrar validadores específicos
        services.AddScoped<IValidadorEmisor, ValidadorEmisor>();
        services.AddScoped<IValidadorReceptor, ValidadorReceptor>();
        services.AddScoped<IValidadorDetalleServicio, ValidadorDetalleServicio>();
        services.AddScoped<IValidadorResumenFactura, ValidadorResumenFactura>();
        services.AddScoped<IValidadorCodigosHacienda, ValidadorCodigosHacienda>();

        // Registrar servicio principal
        services.AddScoped<IServicioValidacionFactura, ServicioValidacionFactura>();

        //services.AddGeneradoresDocumentos();
        services.AddMemoryCache();

        return services;
    }
}