using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Abstracciones.Services;
using FacturaElectronica.Dominio.Modelos;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Infraestructura.Services;

/// <summary>
/// Proveedor de certificados digitales multi-tenant.
/// Cada tenant debe tener su propio certificado configurado en la BD.
/// </summary>
public class CertificadoProvider : ICertificadoProvider
{
    private readonly ITenantCertificateRepository _certificateRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<CertificadoProvider> _logger;

    public CertificadoProvider(
        ITenantCertificateRepository certificateRepository,
        IEncryptionService encryptionService,
        ILogger<CertificadoProvider> logger)
    {
        _certificateRepository = certificateRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<ConfiguracionCertificadoEmpresa> ObtenerCertificadoAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Resolviendo certificado para tenant {TenantId}", tenantId);

        var certConfig = await _certificateRepository.GetByTenantIdAsync(tenantId, cancellationToken);

        if (certConfig is null)
        {
            _logger.LogError("No se encontró certificado activo para tenant {TenantId}", tenantId);
            throw new InvalidOperationException(
                $"No hay certificado digital configurado para el tenant {tenantId}. " +
                "Por favor configure un certificado antes de enviar documentos electrónicos.");
        }

        _logger.LogInformation(
            "Usando certificado {CertPath} para tenant {TenantId}",
            certConfig.CertificatePath, tenantId);

        try
        {
            var claveDesencriptada = _encryptionService.Decrypt(certConfig.CertificateKeyEncrypted);

            return new ConfiguracionCertificadoEmpresa
            {
                TenantId = tenantId,
                NombreCertificado = certConfig.CertificatePath,
                ClaveCertificado = claveDesencriptada
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al desencriptar clave del certificado para tenant {TenantId}.",
                tenantId);

            throw new InvalidOperationException(
                $"Error al desencriptar la clave del certificado para el tenant {tenantId}. " +
                "Verifique que el certificado esté correctamente configurado.", ex);
        }
    }
}
