using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.MarcarCorreccion;

public class MarcarParaCorreccionCommandHandler : IRequestHandler<MarcarParaCorreccionCommand, MarcarParaCorreccionResponse>
{
    private readonly IElectronicInvoiceRepository _invoiceRepository;

    private static readonly HashSet<string> EstadosPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "rechazado",
        "error"
    };

    public MarcarParaCorreccionCommandHandler(IElectronicInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<MarcarParaCorreccionResponse> Handle(MarcarParaCorreccionCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, request.TenantId, cancellationToken);
        if (invoice == null)
        {
            return new MarcarParaCorreccionResponse
            {
                Success = false,
                Message = "Factura electrónica no encontrada"
            };
        }

        if (!EstadosPermitidos.Contains(invoice.Status))
        {
            return new MarcarParaCorreccionResponse
            {
                Success = false,
                Message = $"Solo se pueden marcar para corrección facturas con estado 'rechazado' o 'error'. Estado actual: {invoice.Status}"
            };
        }

        if (invoice.RequiereCorreccion)
        {
            return new MarcarParaCorreccionResponse
            {
                Success = false,
                Message = "La factura ya está marcada para corrección"
            };
        }

        await _invoiceRepository.MarcarParaCorreccionAsync(request.Id, request.TenantId, request.Notas, cancellationToken);

        return new MarcarParaCorreccionResponse
        {
            Success = true,
            Message = "Factura marcada para corrección exitosamente"
        };
    }
}
