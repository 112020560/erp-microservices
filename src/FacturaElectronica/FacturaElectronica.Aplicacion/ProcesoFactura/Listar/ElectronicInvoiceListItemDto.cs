namespace FacturaElectronica.Aplicacion.ProcesoFactura.Listar;

public class ElectronicInvoiceListItemDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? ExternalDocumentId { get; set; }
    public string? TipoDocumento { get; set; }
    public string Status { get; set; } = null!;
    public string? Clave { get; set; }
    public string? Consecutivo { get; set; }
    public string? EmisorIdentificacion { get; set; }
    public string? ReceptorIdentificacion { get; set; }
    public DateTime FechaEmision { get; set; }
    public bool RequiereCorreccion { get; set; }
    public string? Error { get; set; }
}
