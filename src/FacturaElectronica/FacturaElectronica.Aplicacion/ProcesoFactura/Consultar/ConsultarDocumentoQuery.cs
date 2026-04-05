using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.Consultar;

public record ConsultarDocumentoQuery(string Clave, Guid TenantId) : IRequest<ConsultaDocumentoResponse>;
