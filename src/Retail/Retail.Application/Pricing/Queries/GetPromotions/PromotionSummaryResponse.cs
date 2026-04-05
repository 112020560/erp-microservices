namespace Retail.Application.Pricing.Queries.GetPromotions;

public sealed record PromotionSummaryResponse(
    Guid Id,
    string Name,
    string? CouponCode,
    bool IsAutomatic,
    bool IsActive,
    int Priority,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    int? MaxUses,
    int UsedCount,
    bool CanStackWithOthers,
    int ConditionCount,
    int ActionCount);
