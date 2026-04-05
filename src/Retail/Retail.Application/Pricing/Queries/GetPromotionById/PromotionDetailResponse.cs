using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.GetPromotionById;

public sealed record PromotionDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string? CouponCode,
    bool IsAutomatic,
    bool IsActive,
    int Priority,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    int? MaxUses,
    int? MaxUsesPerCustomer,
    int UsedCount,
    bool CanStackWithOthers,
    IReadOnlyList<PromotionConditionResponse> Conditions,
    IReadOnlyList<PromotionActionResponse> Actions);

public sealed record PromotionConditionResponse(
    Guid Id,
    PromotionConditionType ConditionType,
    decimal? DecimalValue,
    Guid? ReferenceId,
    int? IntValue);

public sealed record PromotionActionResponse(
    Guid Id,
    PromotionActionType ActionType,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    Guid? TargetReferenceId,
    int? BuyQuantity,
    int? GetQuantity,
    Guid? BuyReferenceId,
    Guid? GetReferenceId);
