namespace FacturaElectronica.Dominio.Modelos;

/// <summary>
/// Modelo que representa la configuración de certificado digital por tenant.
/// </summary>
public class ConfiguracionCertificadoEmpresa
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Ruta o nombre del archivo de certificado (ej: "empresa.p12").
    /// El archivo debe existir en la carpeta ./Certificados/
    /// </summary>
    public string? NombreCertificado { get; set; }

    /// <summary>
    /// Clave del certificado (desencriptada, lista para usar).
    /// </summary>
    public string? ClaveCertificado { get; set; }

    /// <summary>
    /// Indica si el tenant tiene un certificado configurado.
    /// </summary>
    public bool TieneCertificadoPropio =>
        !string.IsNullOrEmpty(NombreCertificado) &&
        !string.IsNullOrEmpty(ClaveCertificado);
}
