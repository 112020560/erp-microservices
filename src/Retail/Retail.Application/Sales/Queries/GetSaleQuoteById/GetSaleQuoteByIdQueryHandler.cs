using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;
using SharedKernel;

namespace Retail.Application.Sales.Queries.GetSaleQuoteById;

internal sealed class GetSaleQuoteByIdQueryHandler(ISaleQuoteRepository quoteRepository)
    : IQueryHandler<GetSaleQuoteByIdQuery, SaleQuoteDetailResponse>
{
    public async Task<Result<SaleQuoteDetailResponse>> Handle(
        GetSaleQuoteByIdQuery request,
        CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (quote is null) return Result.Failure<SaleQuoteDetailResponse>(SaleErrors.QuoteNotFound);

        var lines = quote.Lines
            .Select(l => new SaleQuoteLineResponse(
                l.Id, l.ProductId, l.Sku, l.ProductName, l.CategoryId,
                l.Quantity, l.UnitPrice, l.DiscountPercentage, l.LineTotal,
                l.PriceListName, l.ResolutionSource))
            .ToList()
            .AsReadOnly() as IReadOnlyList<SaleQuoteLineResponse>;

        var promos = quote.AppliedPromotions
            .Select(p => new AppliedPromotionResponse(p.PromotionId, p.PromotionName, p.DiscountAmount))
            .ToList()
            .AsReadOnly() as IReadOnlyList<AppliedPromotionResponse>;

        var response = new SaleQuoteDetailResponse(
            quote.Id,
            quote.QuoteNumber,
            quote.SalesPersonId,
            quote.CustomerId,
            quote.CustomerName,
            quote.WarehouseId,
            quote.Channel.ToString(),
            quote.Status,
            quote.Currency,
            quote.ValidUntil,
            quote.Notes,
            quote.Subtotal,
            quote.VolumeDiscountAmount,
            quote.PromotionDiscountAmount,
            quote.TaxAmount,
            quote.Total,
            lines!,
            promos!,
            quote.CreatedAt,
            quote.UpdatedAt);

        return Result.Success(response);
    }
}
