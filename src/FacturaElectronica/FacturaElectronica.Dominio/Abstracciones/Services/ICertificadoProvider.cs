using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Abstracciones.Services;

/// <summary>
/// Proveedor de certificados digitales para facturación electrónica.
/// Cada tenant debe tener su propio certificado configurado.
/// </summary>
public interface ICertificadoProvider
{
    /// <summary>
    /// Obtiene la configuración de certificado para un tenant.
    /// Lanza excepción si el tenant no tiene certificado configurado.
    /// </summary>
    /// <param name="tenantId">ID del tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Configuración del certificado a usar</returns>
    Task<ConfiguracionCertificadoEmpresa> ObtenerCertificadoAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
