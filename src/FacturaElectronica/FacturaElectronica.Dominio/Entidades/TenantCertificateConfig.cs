namespace FacturaElectronica.Dominio.Entidades;

public class TenantCertificateConfig
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CertificatePath { get; set; } = string.Empty;
    public string CertificateKeyEncrypted { get; set; } = string.Empty;
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
