namespace Retail.Domain.Pricing;

public sealed class PromotionAction
{
    private PromotionAction() { }

    public Guid Id { get; private set; }
    public Guid PromotionId { get; private set; }
    public PromotionActionType ActionType { get; private set; }
    public decimal? DiscountPercentage { get; private set; }  // for % discounts
    public decimal? DiscountAmount { get; private set; }       // for fixed discounts
    public Guid? TargetReferenceId { get; private set; }       // product for targeted discounts
    public int? BuyQuantity { get; private set; }              // for BuyXGetYFree
    public int? GetQuantity { get; private set; }              // for BuyXGetYFree
    public Guid? BuyReferenceId { get; private set; }          // product to buy for BOGO
    public Guid? GetReferenceId { get; private set; }          // product given free for BOGO
    public DateTimeOffset CreatedAt { get; private set; }

    internal static PromotionAction Create(
        Guid promotionId,
        PromotionActionType actionType,
        decimal? discountPercentage,
        decimal? discountAmount,
        Guid? targetReferenceId,
        int? buyQuantity,
        int? getQuantity,
        Guid? buyReferenceId,
        Guid? getReferenceId)
    {
        return new PromotionAction
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            ActionType = actionType,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            TargetReferenceId = targetReferenceId,
            BuyQuantity = buyQuantity,
            GetQuantity = getQuantity,
            BuyReferenceId = buyReferenceId,
            GetReferenceId = getReferenceId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
