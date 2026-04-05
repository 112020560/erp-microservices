namespace FacturaElectronica.Aplicacion.ProcesoFactura.Listar;

public class ListarFacturasResponse
{
    public List<ElectronicInvoiceListItemDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
