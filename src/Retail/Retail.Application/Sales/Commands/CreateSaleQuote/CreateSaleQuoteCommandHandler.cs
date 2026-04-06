using MassTransit;
using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing.Abstractions;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;
using SharedKernel;
using SharedKernel.Contracts.Sales;

namespace Retail.Application.Sales.Commands.CreateSaleQuote;

internal sealed class CreateSaleQuoteCommandHandler(
    ISaleQuoteRepository quoteRepository,
    ISaleNumberGenerator numberGenerator,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSaleQuoteCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSaleQuoteCommand request, CancellationToken cancellationToken)
    {
        var quoteNumber = await numberGenerator.NextQuoteNumberAsync(cancellationToken);

        var lines = request.Lines.Select(l => new SaleQuoteLineRequest(
            l.ProductId, l.Sku, l.ProductName, l.CategoryId,
            l.Quantity, l.UnitPrice, l.DiscountPercentage, l.LineTotal,
            l.PriceListName, l.ResolutionSource)).ToList();

        var promos = request.AppliedPromotions.Select(p =>
            new AppliedPromotionRequest(p.PromotionId, p.PromotionName, p.DiscountAmount)).ToList();

        var result = SaleQuote.Create(
            quoteNumber, request.SalesPersonId, request.CustomerId, request.CustomerName,
            request.WarehouseId, request.Channel, request.ValidUntil, request.Currency,
            request.Notes, request.Subtotal, request.VolumeDiscountAmount,
            request.PromotionDiscountAmount, request.TaxAmount, request.Total,
            lines, promos);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        var quote = result.Value;
        await quoteRepository.AddAsync(quote, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event → Inventory will create stock reservations
        await publishEndpoint.Publish(new SaleQuoteCreatedEvent
        {
            QuoteId = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            WarehouseId = quote.WarehouseId,
            Lines = quote.Lines.Select(l => new SaleQuoteLineContract(l.ProductId, l.Quantity)).ToList(),
            ValidUntil = quote.ValidUntil,
            CreatedAt = quote.CreatedAt
        }, cancellationToken);

        return Result.Success(quote.Id);
    }
}
