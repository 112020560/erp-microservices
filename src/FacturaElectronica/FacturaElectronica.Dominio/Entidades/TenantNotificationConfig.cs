namespace FacturaElectronica.Dominio.Entidades;

public class TenantNotificationConfig
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.Webhook;
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    public string SubscribedEvents { get; set; } = "document.processed";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
