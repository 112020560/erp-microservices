using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.ApplyPromotions;

public sealed record PromotionsApplicationResult(
    IReadOnlyList<AppliedPromotionResult> AppliedPromotions,
    decimal TotalDiscount,
    decimal FinalTotal);

public sealed record AppliedPromotionResult(
    Guid PromotionId,
    string Name,
    string? CouponCode,
    decimal DiscountAmount,
    IReadOnlyList<AppliedActionResult> Actions);

public sealed record AppliedActionResult(
    PromotionActionType ActionType,
    string Description,
    decimal DiscountAmount);
