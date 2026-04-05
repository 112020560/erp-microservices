using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.DescargarDocumento;

public record DescargarDocumentoQuery(Guid Id, Guid TenantId, string Tipo) : IRequest<DescargarDocumentoResponse?>;

public class DescargarDocumentoResponse
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = "application/xml";
}
