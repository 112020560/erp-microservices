using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.ObtenerDetalle;

public class ObtenerDetalleFacturaQueryHandler : IRequestHandler<ObtenerDetalleFacturaQuery, ElectronicInvoiceDetailDto?>
{
    private readonly IElectronicInvoiceRepository _invoiceRepository;
    private readonly IElectronicDocumentLogRepository _logRepository;

    public ObtenerDetalleFacturaQueryHandler(
        IElectronicInvoiceRepository invoiceRepository,
        IElectronicDocumentLogRepository logRepository)
    {
        _invoiceRepository = invoiceRepository;
        _logRepository = logRepository;
    }

    public async Task<ElectronicInvoiceDetailDto?> Handle(ObtenerDetalleFacturaQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, request.TenantId, cancellationToken);
        if (invoice == null)
            return null;

        var logs = await _logRepository.GetByInvoiceIdAsync(request.Id, cancellationToken);

        return new ElectronicInvoiceDetailDto
        {
            Id = invoice.Id,
            TenantId = invoice.TenantId,
            ExternalDocumentId = invoice.ExternalDocumentId,
            InvoiceType = invoice.InvoiceType,
            Status = invoice.Status,
            StatusDetail = invoice.StatusDetail,
            Clave = invoice.Clave,
            Consecutivo = invoice.Consecutivo,
            EmisorIdentificacion = invoice.EmisorIdentificacion,
            ReceptorIdentificacion = invoice.ReceptorIdentificacion,
            XmlEmisorPath = invoice.XmlEmisorPath,
            XmlReceptorPath = invoice.XmlReceptorPath,
            XmlRespuestaPath = invoice.XmlRespuestaPath,
            FechaEmision = invoice.FechaEmision,
            FechaEnvio = invoice.FechaEnvio,
            ResponseMessage = invoice.ResponseMessage,
            Error = invoice.Error,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,
            ProcessType = invoice.ProcessType,
            RequiereCorreccion = invoice.RequiereCorreccion,
            NotasCorreccion = invoice.NotasCorreccion,
            FechaMarcadoCorreccion = invoice.FechaMarcadoCorreccion,
            Logs = logs.Select(l => new ElectronicDocumentLogDto
            {
                Id = l.Id,
                Action = l.Action,
                Message = l.Message,
                Details = l.Details,
                CreatedAt = l.CreatedAt
            }).ToList()
        };
    }
}
