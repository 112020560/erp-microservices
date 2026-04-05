using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Servicios.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas;

public class ServicioValidacionFactura : IServicioValidacionFactura
{
    private readonly ILogger<ServicioValidacionFactura> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ServicioValidacionFactura(ILogger<ServicioValidacionFactura> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<ResultadoValidacion> ValidarFacturaAsync(Factura factura, VersionFacturaElectronica version = VersionFacturaElectronica.V44)
    {
        _logger.LogInformation("Iniciando validación de factura {ConsecutivoDocumento} versión {Version}", 
            factura.ConsecutivoDocumento, version);

        try
        {
            var validador = ObtenerValidador(version);
            var resultado = validador.Validar(factura);

            _logger.LogInformation("Validación completada. Errores: {Errores}, Advertencias: {Advertencias}", 
                resultado.Errores.Count, resultado.Advertencias.Count);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la validación de factura {ConsecutivoDocumento}", factura.ConsecutivoDocumento);
            return ResultadoValidacion.ConError($"Error interno durante validación: {ex.Message}");
        }
    }

    public async Task<ResultadoValidacion> ValidarFacturaCompletaAsync(Factura factura, VersionFacturaElectronica version = VersionFacturaElectronica.V44)
    {
        var resultado = await ValidarFacturaAsync(factura, version);

        // Validaciones adicionales asíncronas (ej: validar contra servicios externos)
        if (resultado.EsValido)
        {
            await ValidarDatosExternosAsync(factura, resultado);
        }

        return resultado;
    }

    private IValidador<Factura> ObtenerValidador(VersionFacturaElectronica version)
    {
        return version switch
        {
            VersionFacturaElectronica.V44 => _serviceProvider.GetRequiredService<ValidadorFacturaV44>(),
            VersionFacturaElectronica.V43 => throw new NotImplementedException("Validador v4.3 no implementado"),
            VersionFacturaElectronica.V42 => throw new NotImplementedException("Validador v4.2 no implementado"),
            _ => throw new ArgumentException($"Versión {version} no soportada")
        };
    }

    private async Task ValidarDatosExternosAsync(Factura factura, ResultadoValidacion resultado)
    {
        try
        {
            // Aquí podrías validar datos contra servicios externos como:
            // - API de Hacienda para validar contribuyente
            // - Servicio de geocodificación para validar direcciones
            // - APIs de bancos para validar medios de pago
            
            await Task.Delay(1); // Placeholder para validaciones asíncronas
            
            _logger.LogDebug("Validaciones externas completadas para factura {ConsecutivoDocumento}", 
                factura.ConsecutivoDocumento);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error en validaciones externas para factura {ConsecutivoDocumento}", 
                factura.ConsecutivoDocumento);
            resultado.AgregarAdvertencia("No se pudieron completar las validaciones externas");
        }
    }
}