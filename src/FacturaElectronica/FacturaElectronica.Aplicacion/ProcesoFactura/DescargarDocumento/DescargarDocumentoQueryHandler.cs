using System.Text;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.DescargarDocumento;

public class DescargarDocumentoQueryHandler : IRequestHandler<DescargarDocumentoQuery, DescargarDocumentoResponse?>
{
    private readonly IElectronicInvoiceRepository _invoiceRepository;
    private readonly IServicioAlmacenamientoDocumentos _storageService;

    public DescargarDocumentoQueryHandler(
        IElectronicInvoiceRepository invoiceRepository,
        IServicioAlmacenamientoDocumentos storageService)
    {
        _invoiceRepository = invoiceRepository;
        _storageService = storageService;
    }

    public async Task<DescargarDocumentoResponse?> Handle(DescargarDocumentoQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, request.TenantId, cancellationToken);
        if (invoice == null)
            return null;

        var documento = await _storageService.ObtenerDocumentoAsync(invoice.Clave!, cancellationToken);
        if (documento == null)
            return null;

        string? xmlContent = request.Tipo.ToLower() switch
        {
            "sin-firmar" => documento.XmlSinFirmar,
            "firmado" => documento.XmlFirmado,
            "respuesta" => documento.XmlRespuesta,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(xmlContent))
            return null;

        var fileName = $"{invoice.Clave}_{request.Tipo}.xml";

        return new DescargarDocumentoResponse
        {
            Content = Encoding.UTF8.GetBytes(xmlContent),
            FileName = fileName,
            ContentType = "application/xml"
        };
    }
}
