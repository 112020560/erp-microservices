namespace Retail.Domain.Pricing;

public sealed record PriceResolutionResult(
    Guid ProductId,
    decimal BasePrice,
    decimal DiscountPercentage,
    decimal FinalPrice,
    bool PriceIncludesTax,
    string? PriceListName,
    string Currency,
    string ResolutionSource); // "Customer" | "Channel" | "Default"
