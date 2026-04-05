using SharedKernel;

namespace Retail.Domain.Pricing;

public static class PromotionErrors
{
    public static readonly Error NameRequired = Error.Failure("Promotion.NameRequired", "Promotion name is required.");
    public static readonly Error InvalidDateRange = Error.Failure("Promotion.InvalidDateRange", "ValidTo must be after ValidFrom.");
    public static readonly Error NotFound = Error.NotFound("Promotion.NotFound", "Promotion not found.");
    public static readonly Error AlreadyActive = Error.Failure("Promotion.AlreadyActive", "Promotion is already active.");
    public static readonly Error AlreadyInactive = Error.Failure("Promotion.AlreadyInactive", "Promotion is already inactive.");
    public static readonly Error MaxUsesReached = Error.Failure("Promotion.MaxUsesReached", "This promotion has reached its maximum number of uses.");
    public static readonly Error MaxUsesPerCustomerReached = Error.Failure("Promotion.MaxUsesPerCustomerReached", "This customer has reached the maximum uses for this promotion.");
    public static readonly Error InvalidCouponCode = Error.NotFound("Promotion.InvalidCouponCode", "The coupon code is invalid or has expired.");

    // Condition errors
    public static readonly Error ConditionReferenceIdRequired = Error.Failure("Promotion.Condition.ReferenceIdRequired", "ReferenceId is required for this condition type.");
    public static readonly Error ConditionValueRequired = Error.Failure("Promotion.Condition.ValueRequired", "A positive value is required for this condition type.");
    public static readonly Error ConditionNotFound = Error.NotFound("Promotion.Condition.NotFound", "Condition not found.");

    // Action errors
    public static readonly Error ActionInvalidPercentage = Error.Failure("Promotion.Action.InvalidPercentage", "Discount percentage must be between 0 and 100.");
    public static readonly Error ActionInvalidAmount = Error.Failure("Promotion.Action.InvalidAmount", "Discount amount must be positive.");
    public static readonly Error ActionTargetRequired = Error.Failure("Promotion.Action.TargetRequired", "Target product reference is required for this action type.");
    public static readonly Error ActionInvalidBogoQuantities = Error.Failure("Promotion.Action.InvalidBogoQuantities", "Buy and Get quantities must be positive for BuyXGetYFree.");
    public static readonly Error ActionNotFound = Error.NotFound("Promotion.Action.NotFound", "Action not found.");
}
