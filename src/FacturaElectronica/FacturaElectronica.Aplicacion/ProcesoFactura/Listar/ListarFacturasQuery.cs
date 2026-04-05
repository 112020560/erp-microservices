using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.Listar;

public record ListarFacturasQuery(
    Guid TenantId,
    string? Status = null,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null,
    string? EmisorId = null,
    string? ReceptorId = null,
    bool? RequiereCorreccion = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<ListarFacturasResponse>;
