namespace FacturaElectronica.Dominio.Entidades;

public class TenantHaciendaConfig
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Environment { get; set; } = "sandbox"; // "sandbox" | "production"
    public string ClientId { get; set; } = string.Empty;
    public string UsernameEncrypted { get; set; } = string.Empty;
    public string PasswordEncrypted { get; set; } = string.Empty;
    public string AuthUrl { get; set; } = string.Empty;
    public string SubmitUrl { get; set; } = string.Empty;
    public string QueryUrl { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public string? CallbackUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
