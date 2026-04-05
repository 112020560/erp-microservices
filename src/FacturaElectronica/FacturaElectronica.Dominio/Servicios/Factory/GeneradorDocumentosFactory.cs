using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Factory;

public enum VersionFacturaElectronica
{
    V42,
    V43,
    V44
}

public interface IGeneradorDocumentosFactory
{
    IGeneradorDocumentos CrearGenerador(VersionFacturaElectronica version);

    /// <summary>
    /// Crea el generador apropiado según el tipo de documento.
    /// </summary>
    /// <param name="tipoDocumento">Tipo de documento: "01" = Factura, "04" = Tiquete</param>
    /// <param name="version">Versión del esquema XML</param>
    /// <returns>Generador de documentos apropiado</returns>
    IGeneradorDocumentos CrearGeneradorPorTipoDocumento(string tipoDocumento, VersionFacturaElectronica version);
}

public class GeneradorDocumentosFactory : IGeneradorDocumentosFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GeneradorDocumentosFactory> _logger;

    public GeneradorDocumentosFactory(IServiceProvider serviceProvider, 
        ILogger<GeneradorDocumentosFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IGeneradorDocumentos CrearGenerador(VersionFacturaElectronica version)
    {
        _logger.LogInformation("Creando generador para versión {Version}", version);

        return version switch
        {
            VersionFacturaElectronica.V42 => _serviceProvider.GetRequiredService<GeneradorDocumentos>(),
            VersionFacturaElectronica.V43 => _serviceProvider.GetRequiredService<GeneradorDocumentos>(), // Puedes crear uno específico
            VersionFacturaElectronica.V44 => _serviceProvider.GetRequiredService<GeneradorDocumentosV44>(),
            _ => throw new NotSupportedException($"Versión {version} no soportada")
        };
    }

    public IGeneradorDocumentos CrearGeneradorPorTipoDocumento(string tipoDocumento, VersionFacturaElectronica version)
    {
        _logger.LogInformation("Creando generador para tipo documento {TipoDocumento}, versión {Version}",
            tipoDocumento, version);

        // Para v4.4, seleccionar generador según tipo de documento
        if (version == VersionFacturaElectronica.V44)
        {
            return tipoDocumento switch
            {
                "01" => _serviceProvider.GetRequiredService<GeneradorDocumentosV44>(), // Factura Electrónica
                "02" => _serviceProvider.GetRequiredService<GeneradorNotaDebitoV44>(), // Nota de Débito
                "03" => _serviceProvider.GetRequiredService<GeneradorNotaCreditoV44>(), // Nota de Crédito
                "04" => _serviceProvider.GetRequiredService<GeneradorTiqueteV44>(),    // Tiquete Electrónico
                _ => _serviceProvider.GetRequiredService<GeneradorDocumentosV44>()     // Default: Factura
            };
        }

        // Para otras versiones, usar el generador estándar
        return CrearGenerador(version);
    }
}