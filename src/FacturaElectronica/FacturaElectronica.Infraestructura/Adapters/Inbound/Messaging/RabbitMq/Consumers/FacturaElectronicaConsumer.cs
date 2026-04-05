using FacturaElectronica.Aplicacion.ProcesoFactura.Enviar;
using MassTransit;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Infraestructura.Adapters.Inbound.Messaging.RabbitMq.Consumers;

public class FacturaElectronicaConsumer(
    ILogger<FacturaElectronicaConsumer> logger,
    IMediator mediator) : IConsumer<FacturaElectronicaContract>
{
    public async Task Consume(ConsumeContext<FacturaElectronicaContract> context)
    {
        var tenantId = context.Message.TenantId;
        var externalDocumentId = context.Message.ExternalDocumentId;
        var consecutivo = context.Message.ConsecutivoDocumento;

        logger.LogInformation(
            "Mensaje de factura electrónica recibido - TenantId: {TenantId}, ExternalDocumentId: {ExternalDocumentId}, Consecutivo: {Consecutivo}",
            tenantId, externalDocumentId, consecutivo);

        var facturaRequest = context.Message.Adapt<ProcesoFacturaRequest>();
        facturaRequest.TenantId = tenantId;
        facturaRequest.ExternalDocumentId = externalDocumentId;

        var command = new EnviarFacturaCommand(facturaRequest);
        await mediator.Send(command, context.CancellationToken);
    }
}
