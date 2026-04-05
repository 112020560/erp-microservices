using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.Listar;

public class ListarFacturasQueryHandler : IRequestHandler<ListarFacturasQuery, ListarFacturasResponse>
{
    private readonly IElectronicInvoiceRepository _repository;

    public ListarFacturasQueryHandler(IElectronicInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListarFacturasResponse> Handle(ListarFacturasQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            request.TenantId,
            request.Status,
            request.FechaDesde,
            request.FechaHasta,
            request.EmisorId,
            request.ReceptorId,
            request.RequiereCorreccion,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtoItems = items.Select(x => new ElectronicInvoiceListItemDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            ExternalDocumentId = x.ExternalDocumentId,
            Status = x.Status,
            Clave = x.Clave,
            Consecutivo = x.Consecutivo,
            EmisorIdentificacion = x.EmisorIdentificacion,
            ReceptorIdentificacion = x.ReceptorIdentificacion,
            FechaEmision = x.FechaEmision,
            RequiereCorreccion = x.RequiereCorreccion,
            Error = x.Error,
            TipoDocumento = GetInvoiceType(x.InvoiceType)
        }).ToList();

        return new ListarFacturasResponse
        {
            Items = dtoItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    private string GetInvoiceType(string invoiceType)
    {
        return invoiceType switch
        {
            "01" => "Factura Electrónica",
            "02" => "Nota de Débito Electrónica",
            "04" => "Tiquete Electrónico",
            "03" => "Nota de Crédito Electrónica",
            _ => "Desconocido"
        };
    }
}
