using MassTransit;
using Microsoft.Extensions.Logging;
using Retail.Domain.Pricing.Abstractions;
using Retail.Domain.Sales.Abstractions;
using SharedKernel.Contracts.ElectronicInvoice;

namespace Retail.Infrastructure.Messaging.Consumers;

public sealed class ElectronicDocumentProcessedConsumer(
    ISaleInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ILogger<ElectronicDocumentProcessedConsumer> logger) : IConsumer<ElectronicDocumentProcessedEvent>
{
    public async Task Consume(ConsumeContext<ElectronicDocumentProcessedEvent> context)
    {
        var msg = context.Message;

        if (string.IsNullOrWhiteSpace(msg.ExternalDocumentId))
        {
            logger.LogWarning(
                "ElectronicDocumentProcessedEvent for document {DocumentId} has no ExternalDocumentId — skipping",
                msg.DocumentId);
            return;
        }

        var invoice = await invoiceRepository.GetByInvoiceNumberAsync(msg.ExternalDocumentId, context.CancellationToken);
        if (invoice is null)
        {
            logger.LogWarning(
                "SaleInvoice with number {InvoiceNumber} not found — skipping",
                msg.ExternalDocumentId);
            return;
        }

        if (invoice.ElectronicDocumentId.HasValue)
        {
            logger.LogInformation(
                "SaleInvoice {InvoiceNumber} already linked to ElectronicDocument {DocId} — skipping",
                msg.ExternalDocumentId, invoice.ElectronicDocumentId);
            return;
        }

        invoice.SetElectronicDocumentId(msg.DocumentId);
        invoiceRepository.Update(invoice);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "SaleInvoice {InvoiceNumber} linked to ElectronicDocument {DocumentId} (Status: {Status})",
            msg.ExternalDocumentId, msg.DocumentId, msg.Status);
    }
}
