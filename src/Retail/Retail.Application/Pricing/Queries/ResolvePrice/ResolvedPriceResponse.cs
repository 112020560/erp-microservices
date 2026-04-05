namespace Retail.Application.Pricing.Queries.ResolvePrice;

public sealed record ResolvedPriceResponse(
    Guid ProductId,
    decimal BasePrice,
    decimal DiscountPercentage,
    decimal FinalPrice,
    bool PriceIncludesTax,
    string? PriceListName,
    string Currency,
    string ResolutionSource);
