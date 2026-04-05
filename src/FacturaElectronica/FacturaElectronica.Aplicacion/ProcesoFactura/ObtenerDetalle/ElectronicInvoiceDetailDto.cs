namespace FacturaElectronica.Aplicacion.ProcesoFactura.ObtenerDetalle;

public class ElectronicInvoiceDetailDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? ExternalDocumentId { get; set; }
    public string InvoiceType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? StatusDetail { get; set; }
    public string? Clave { get; set; }
    public string? Consecutivo { get; set; }
    public string? EmisorIdentificacion { get; set; }
    public string? ReceptorIdentificacion { get; set; }
    public string? XmlEmisorPath { get; set; }
    public string? XmlReceptorPath { get; set; }
    public string? XmlRespuestaPath { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public string? ResponseMessage { get; set; }
    public string? Error { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ProcessType { get; set; }
    public bool RequiereCorreccion { get; set; }
    public string? NotasCorreccion { get; set; }
    public DateTime? FechaMarcadoCorreccion { get; set; }
    public List<ElectronicDocumentLogDto> Logs { get; set; } = new();
}

public class ElectronicDocumentLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? Message { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
