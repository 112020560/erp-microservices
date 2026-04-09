using MassTransit;
using Microsoft.Extensions.Logging;
using Retail.Application.Abstractions.Messaging;
using Retail.Application.Abstractions.Services;
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
    IPromotionRepository promotionRepository,
    ICreditServiceClient creditServiceClient,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork,
    ILogger<CreateSaleInvoiceCommandHandler> logger)
    : ICommandHandler<CreateSaleInvoiceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSaleInvoiceCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdWithDetailsAsync(request.QuoteId, cancellationToken);
        if (quote is null) return Result.Failure<Guid>(SaleErrors.QuoteNotFound);

        // Validate credit payment
        var hasCreditPayment = request.Payments.Any(p => p.Method == PaymentMethod.Credit);
        if (hasCreditPayment)
        {
            if (quote.CustomerId is null)
                return Result.Failure<Guid>(SaleErrors.CreditRequiresCustomer);

            var creditStatus = await creditServiceClient.GetCustomerCreditStatusAsync(
                quote.CustomerId.Value, cancellationToken);

            if (creditStatus is null)
                return Result.Failure<Guid>(SaleErrors.CreditServiceUnavailable);

            if (!creditStatus.CustomerExists)
                return Result.Failure<Guid>(SaleErrors.CustomerNotInCreditSystem);
        }

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

        // Record promotion usages (best-effort: don't fail invoice if a limit was exceeded after quote was created)
        if (quote.AppliedPromotions.Count > 0)
        {
            var promotionIds = quote.AppliedPromotions.Select(p => p.PromotionId).ToList();
            var promotions = await promotionRepository.GetByIdsWithUsagesAsync(promotionIds, cancellationToken);

            foreach (var promotion in promotions)
            {
                var result = promotion.RecordUsage(quote.CustomerId, invoice.InvoiceNumber);
                if (result.IsFailure)
                    logger.LogWarning(
                        "RecordUsage skipped for promotion {PromotionId} on invoice {InvoiceNumber}: {Error}",
                        promotion.Id, invoice.InvoiceNumber, result.Error.Description);
                else
                    promotionRepository.Update(promotion);
            }
        }

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
            ConfirmedAt = invoice.CreatedAt,
            CreditAmount = request.Payments
                .Where(p => p.Method == PaymentMethod.Credit)
                .Sum(p => p.Amount),
            CreditProductId = request.CreditProductId
        }, cancellationToken);

        return Result.Success(invoice.Id);
    }
}
