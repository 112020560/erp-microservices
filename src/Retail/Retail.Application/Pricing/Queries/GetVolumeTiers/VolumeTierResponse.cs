using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.GetVolumeTiers;

public sealed record VolumeTierResponse(
    Guid Id,
    PriceItemType ItemType,
    Guid? ReferenceId,
    decimal MinQuantity,
    decimal? MaxQuantity,
    decimal Price,
    decimal DiscountPercentage,
    decimal? MinPrice,
    bool PriceIncludesTax);
