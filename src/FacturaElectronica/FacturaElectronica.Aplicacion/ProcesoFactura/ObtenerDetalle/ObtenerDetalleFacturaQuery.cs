using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.ObtenerDetalle;

public record ObtenerDetalleFacturaQuery(Guid Id, Guid TenantId) : IRequest<ElectronicInvoiceDetailDto?>;
