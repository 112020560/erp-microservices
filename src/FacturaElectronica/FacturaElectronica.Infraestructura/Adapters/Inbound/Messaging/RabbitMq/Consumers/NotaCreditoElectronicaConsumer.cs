using FacturaElectronica.Aplicacion.ProcesoNotaCredito.Enviar;
using MassTransit;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Infraestructura.Adapters.Inbound.Messaging.RabbitMq.Consumers;

public class NotaCreditoElectronicaConsumer(
    ILogger<NotaCreditoElectronicaConsumer> logger,
    IMediator mediator) : IConsumer<NotaCreditoElectronicaContract>
{
    public async Task Consume(ConsumeContext<NotaCreditoElectronicaContract> context)
    {
        var tenantId = context.Message.TenantId;
        var externalDocumentId = context.Message.ExternalDocumentId;
        var consecutivo = context.Message.ConsecutivoDocumento;

        logger.LogInformation(
            "Mensaje de nota de crédito electrónica recibido - TenantId: {TenantId}, ExternalDocumentId: {ExternalDocumentId}, Consecutivo: {Consecutivo}",
            tenantId, externalDocumentId, consecutivo);

        var notaCreditoRequest = MapearContratoARequest(context.Message);
        notaCreditoRequest.TenantId = tenantId;
        notaCreditoRequest.ExternalDocumentId = externalDocumentId;

        var command = new EnviarNotaCreditoCommand(notaCreditoRequest);
        await mediator.Send(command, context.CancellationToken);
    }

    private static ProcesoNotaCreditoRequest MapearContratoARequest(NotaCreditoElectronicaContract contrato)
    {
        var request = contrato.Adapt<ProcesoNotaCreditoRequest>();
        request.TipoDocumento = "03"; // Asegurar tipo de documento

        // Mapear información de referencia
        request.InformacionReferencia = new InformacionReferenciaRequest
        {
            TipoDoc = contrato.InformacionReferencia.TipoDoc,
            Numero = contrato.InformacionReferencia.Numero,
            FechaEmision = contrato.InformacionReferencia.FechaEmision,
            Codigo = contrato.InformacionReferencia.Codigo,
            Razon = contrato.InformacionReferencia.Razon
        };

        // Mapear detalles de servicios
        if (contrato.DetalleServicios != null)
        {
            request.DetalleServicios = contrato.DetalleServicios.Select(d => new DetalleServicioNotaCreditoRequest
            {
                NumeroLinea = d.NumeroLinea,
                ArticuloTipo = d.ArticuloTipo,
                CodigoArticulo = d.CodigoArticulo,
                Cantidad = d.Cantidad,
                UnidadMedida = d.UnidadMedida,
                DetalleArticulo = d.DetalleArticulo,
                PrecioUnitario = d.PrecioUnitario,
                Precio = d.Precio,
                Descuento = d.Descuento,
                NaturalezaDescuento = d.NaturalezaDescuento,
                SubTotal = d.SubTotal,
                CodigoImpuesto = d.CodigoImpuesto,
                TarifaImpuesto = d.TarifaImpuesto,
                MontoImpuesto = d.MontoImpuesto,
                MontoTotalLinea = d.MontoTotalLinea
            }).ToList();
        }

        return request;
    }
}
