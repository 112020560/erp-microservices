using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.GetScheduledPriceChanges;

public sealed record ScheduledPriceChangeResponse(
    Guid Id,
    Guid PriceListId,
    Guid ItemId,
    decimal NewPrice,
    decimal? NewDiscountPercentage,
    decimal? NewMinPrice,
    DateTimeOffset EffectiveAt,
    ScheduledPriceChangeStatus Status,
    DateTimeOffset? AppliedAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset CreatedAt);
