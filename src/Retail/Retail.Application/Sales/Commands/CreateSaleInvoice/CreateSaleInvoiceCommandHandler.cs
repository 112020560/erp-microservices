using MassTransit;
using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing.Abstractions;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;
using SharedKernel;
using SharedKernel.Contracts.Sales;

namespace Retail.Application.Sales.Commands.CreateSaleInvoice;

internal sealed class CreateSaleInvoiceCommandHandler(
    ISaleQuoteRepository quoteRepository,
    ISaleInvoiceRepository invoiceRepository,
    ISaleNumberGenerator numberGenerator,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSaleInvoiceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSaleInvoiceCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdWithDetailsAsync(request.QuoteId, cancellationToken);
        if (quote is null) return Result.Failure<Guid>(SaleErrors.QuoteNotFound);

        var confirmResult = quote.MarkInvoiced();
        if (confirmResult.IsFailure) return Result.Failure<Guid>(confirmResult.Error);

        var invoiceNumber = await numberGenerator.NextInvoiceNumberAsync(cancellationToken);

        var payments = request.Payments
            .Select(p => new PaymentLineRequest(p.Method, p.Amount, p.Reference))
            .ToList();

        var invoiceResult = SaleInvoice.Create(
            invoiceNumber, quote.Id, request.CashierId,
            request.RequiresElectronicInvoice, quote.Total, payments);

        if (invoiceResult.IsFailure) return Result.Failure<Guid>(invoiceResult.Error);

        var invoice = invoiceResult.Value;

        quoteRepository.Update(quote);
        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish → Inventory deducts stock + FacturaElectronica (if required)
        await publishEndpoint.Publish(new SaleInvoiceConfirmedEvent
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            QuoteId = quote.Id,
            WarehouseId = quote.WarehouseId,
            Lines = quote.Lines.Select(l => new SaleInvoiceLineContract(
                l.ProductId, l.Sku, l.ProductName, l.Quantity, l.UnitPrice, l.LineTotal)).ToList(),
            RequiresElectronicInvoice = invoice.RequiresElectronicInvoice,
            TenantId = request.TenantId,
            CustomerId = quote.CustomerId,
            CustomerName = quote.CustomerName,
            Total = invoice.Total,
            Currency = quote.Currency,
            ConfirmedAt = invoice.CreatedAt
        }, cancellationToken);

        return Result.Success(invoice.Id);
    }
}
