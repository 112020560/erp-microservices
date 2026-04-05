namespace Retail.Application.Pricing.Queries.ResolveCart;

public sealed record CartResolutionResponse(
    IReadOnlyList<CartItemResolution> Items,
    decimal Subtotal,
    decimal? OrderDiscount,
    decimal DiscountAmount,
    decimal FinalTotal,
    string Currency,
    string? PriceListName);

public sealed record CartItemResolution(
    Guid ProductId,
    decimal UnitPrice,
    decimal DiscountPercentage,
    decimal LineTotal,
    bool PriceIncludesTax,
    string ResolutionSource);
