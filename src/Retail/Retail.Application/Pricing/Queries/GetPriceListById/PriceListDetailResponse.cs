using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.GetPriceListById;

public sealed record PriceListDetailResponse(
    Guid Id,
    string Name,
    string Currency,
    int Priority,
    bool IsActive,
    RoundingRule RoundingRule,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    IReadOnlyList<PriceListItemResponse> Items);

public sealed record PriceListItemResponse(
    Guid Id,
    PriceItemType ItemType,
    Guid? ReferenceId,
    decimal MinQuantity,
    decimal? MaxQuantity,
    decimal Price,
    decimal DiscountPercentage,
    decimal? MinPrice,
    bool PriceIncludesTax);
