namespace FacturaElectronica.Dominio.Entidades;

public class ElectronicDocumentLog
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Action { get; set; } = null!;

    public string? Message { get; set; }

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }
}
