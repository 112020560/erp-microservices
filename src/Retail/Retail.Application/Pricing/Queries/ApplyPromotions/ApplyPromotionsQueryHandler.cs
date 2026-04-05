using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.ApplyPromotions;

internal sealed class ApplyPromotionsQueryHandler(IPromotionRepository repository)
    : IQueryHandler<ApplyPromotionsQuery, PromotionsApplicationResult>
{
    public async Task<Result<PromotionsApplicationResult>> Handle(
        ApplyPromotionsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        // Step 1: Collect candidate promotions
        var candidates = new List<Promotion>();

        // 1a. All active automatic promotions valid at now
        var automaticPromotions = await repository.GetActiveAutomaticAsync(now, cancellationToken);
        candidates.AddRange(automaticPromotions);

        // 1b. If CouponCode provided: find by coupon code, validate active and valid
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var couponPromotion = await repository.GetByCouponCodeAsync(
                request.CouponCode.Trim().ToUpperInvariant(),
                cancellationToken);

            if (couponPromotion is not null && couponPromotion.IsValidAt(now))
                candidates.Add(couponPromotion);
        }

        // Step 2: Evaluate conditions for each candidate promotion
        var passingPromotions = candidates
            .Where(p => EvaluateConditions(p, request))
            .ToList();

        // Step 3: Determine which promotions to apply
        var stackable = passingPromotions.Where(p => p.CanStackWithOthers).ToList();
        var nonStackable = passingPromotions
            .Where(p => !p.CanStackWithOthers)
            .OrderByDescending(p => p.Priority)
            .ToList();

        var promotionsToApply = new List<Promotion>();
        promotionsToApply.AddRange(stackable);

        // Take only highest-priority non-stackable
        if (nonStackable.Count > 0)
            promotionsToApply.Add(nonStackable[0]);

        // Step 4 & 5: Calculate discounts for each applied promotion
        var appliedResults = new List<AppliedPromotionResult>();
        var totalDiscount = 0m;

        foreach (var promotion in promotionsToApply)
        {
            var actionResults = new List<AppliedActionResult>();
            var promotionDiscount = 0m;

            foreach (var action in promotion.Actions)
            {
                var (description, discount) = CalculateActionDiscount(action, request);
                actionResults.Add(new AppliedActionResult(action.ActionType, description, discount));
                promotionDiscount += discount;
            }

            totalDiscount += promotionDiscount;
            appliedResults.Add(new AppliedPromotionResult(
                promotion.Id,
                promotion.Name,
                promotion.CouponCode,
                promotionDiscount,
                actionResults.AsReadOnly()));
        }

        // Step 6: FinalTotal = Subtotal - TotalDiscount
        var finalTotal = request.Subtotal - totalDiscount;

        return Result.Success(new PromotionsApplicationResult(
            appliedResults.AsReadOnly(),
            totalDiscount,
            finalTotal));
    }

    private static bool EvaluateConditions(Promotion promotion, ApplyPromotionsQuery request)
    {
        foreach (var condition in promotion.Conditions)
        {
            var passes = condition.ConditionType switch
            {
                PromotionConditionType.MinCartTotal =>
                    request.Subtotal >= (condition.DecimalValue ?? 0),

                PromotionConditionType.MinCartQuantity =>
                    request.TotalQuantity >= (condition.DecimalValue ?? 0),

                PromotionConditionType.ContainsProduct =>
                    request.Items.Any(i => i.ProductId == condition.ReferenceId),

                PromotionConditionType.ContainsCategory =>
                    request.Items.Any(i => i.CategoryId == condition.ReferenceId),

                PromotionConditionType.CustomerInGroup =>
                    condition.ReferenceId.HasValue && request.CustomerGroupIds.Contains(condition.ReferenceId.Value),

                PromotionConditionType.MinItemCount =>
                    request.Items.Count(i => i.Quantity > 0) >= (condition.IntValue ?? 0),

                _ => false
            };

            if (!passes) return false;
        }

        return true;
    }

    private static (string Description, decimal Discount) CalculateActionDiscount(
        PromotionAction action,
        ApplyPromotionsQuery request)
    {
        return action.ActionType switch
        {
            PromotionActionType.CartPercentageDiscount => CalculateCartPercentageDiscount(action, request),
            PromotionActionType.CartFixedDiscount => CalculateCartFixedDiscount(action, request),
            PromotionActionType.ProductPercentageDiscount => CalculateProductPercentageDiscount(action, request),
            PromotionActionType.ProductFixedDiscount => CalculateProductFixedDiscount(action, request),
            PromotionActionType.BuyXGetYFree => CalculateBuyXGetYFree(action, request),
            _ => ("Unknown action", 0m)
        };
    }

    private static (string Description, decimal Discount) CalculateCartPercentageDiscount(
        PromotionAction action, ApplyPromotionsQuery request)
    {
        var discount = request.Subtotal * (action.DiscountPercentage ?? 0) / 100m;
        return ($"{action.DiscountPercentage}% off cart subtotal", discount);
    }

    private static (string Description, decimal Discount) CalculateCartFixedDiscount(
        PromotionAction action, ApplyPromotionsQuery request)
    {
        var discount = Math.Min(action.DiscountAmount ?? 0, request.Subtotal);
        return ($"{action.DiscountAmount} off cart subtotal", discount);
    }

    private static (string Description, decimal Discount) CalculateProductPercentageDiscount(
        PromotionAction action, ApplyPromotionsQuery request)
    {
        var matchingLineTotal = request.Items
            .Where(i => i.ProductId == action.TargetReferenceId)
            .Sum(i => i.LineTotal);

        var discount = matchingLineTotal * (action.DiscountPercentage ?? 0) / 100m;
        return ($"{action.DiscountPercentage}% off product {action.TargetReferenceId}", discount);
    }

    private static (string Description, decimal Discount) CalculateProductFixedDiscount(
        PromotionAction action, ApplyPromotionsQuery request)
    {
        var matchingLineTotal = request.Items
            .Where(i => i.ProductId == action.TargetReferenceId)
            .Sum(i => i.LineTotal);

        var discount = Math.Min(action.DiscountAmount ?? 0, matchingLineTotal);
        return ($"{action.DiscountAmount} off product {action.TargetReferenceId}", discount);
    }

    private static (string Description, decimal Discount) CalculateBuyXGetYFree(
        PromotionAction action, ApplyPromotionsQuery request)
    {
        // Find items matching BuyReferenceId with qty >= BuyQuantity
        var buyItem = request.Items.FirstOrDefault(i => i.ProductId == action.BuyReferenceId);
        if (buyItem is null || buyItem.Quantity < (action.BuyQuantity ?? 0))
            return ("Buy X Get Y Free (conditions not met)", 0m);

        // Value = unit price * getQuantity (unit price = lineTotal / quantity)
        var getItem = request.Items.FirstOrDefault(i => i.ProductId == action.GetReferenceId);
        if (getItem is null || getItem.Quantity <= 0)
            return ("Buy X Get Y Free (get product not in cart)", 0m);

        var unitPrice = getItem.LineTotal / getItem.Quantity;
        var discount = unitPrice * (action.GetQuantity ?? 0);

        return ($"Buy {action.BuyQuantity} of {action.BuyReferenceId}, get {action.GetQuantity} of {action.GetReferenceId} free", discount);
    }
}
