using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.GetPriceLists;

public sealed record PriceListSummaryResponse(
    Guid Id,
    string Name,
    string Currency,
    int Priority,
    bool IsActive,
    RoundingRule RoundingRule,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    int ItemCount);
