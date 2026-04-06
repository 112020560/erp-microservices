using Retail.Domain.Sales;
namespace Retail.Application.Sales.Queries.GetSaleQuoteById;

public sealed record SaleQuoteDetailResponse(
    Guid Id, string QuoteNumber, Guid SalesPersonId,
    Guid? CustomerId, string CustomerName, Guid WarehouseId,
    string Channel, SaleQuoteStatus Status, string Currency,
    DateTimeOffset ValidUntil, string? Notes,
    decimal Subtotal, decimal VolumeDiscountAmount, decimal PromotionDiscountAmount,
    decimal TaxAmount, decimal Total,
    IReadOnlyList<SaleQuoteLineResponse> Lines,
    IReadOnlyList<AppliedPromotionResponse> AppliedPromotions,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record SaleQuoteLineResponse(
    Guid Id, Guid ProductId, string Sku, string ProductName, Guid? CategoryId,
    decimal Quantity, decimal UnitPrice, decimal DiscountPercentage, decimal LineTotal,
    string? PriceListName, string? ResolutionSource);

public sealed record AppliedPromotionResponse(Guid PromotionId, string PromotionName, decimal DiscountAmount);
