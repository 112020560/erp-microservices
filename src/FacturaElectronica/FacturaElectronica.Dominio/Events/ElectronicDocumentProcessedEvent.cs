namespace FacturaElectronica.Dominio.Events;

public record ElectronicDocumentProcessedEvent
{
    public Guid TenantId { get; init; }
    public Guid DocumentId { get; init; }
    public string? ExternalDocumentId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Clave { get; init; }
    public string? Consecutivo { get; init; }
    public string? ResponseMessage { get; init; }
    public string? Error { get; init; }
    public DateTime ProcessedAt { get; init; }
}
