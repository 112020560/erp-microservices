namespace FacturaElectronica.Dominio.Entidades;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string TaxIdType { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TenantEmitterConfig? EmitterConfig { get; set; }
    public TenantCertificateConfig? CertificateConfig { get; set; }
    public TenantHaciendaConfig? HaciendaConfig { get; set; }
    public TenantNotificationConfig? NotificationConfig { get; set; }
}
