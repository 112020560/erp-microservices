namespace Retail.Domain.Pricing;

public sealed record VolumeTierRequest(
    decimal MinQuantity,
    decimal? MaxQuantity,
    decimal Price,
    decimal DiscountPercentage,
    decimal? MinPrice,
    bool PriceIncludesTax);
