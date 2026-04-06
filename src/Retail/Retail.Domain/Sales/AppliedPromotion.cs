namespace Retail.Domain.Sales;

public sealed class AppliedPromotion
{
    private AppliedPromotion() { }

    public Guid Id { get; private set; }
    public Guid QuoteId { get; private set; }
    public Guid PromotionId { get; private set; }
    public string PromotionName { get; private set; } = string.Empty;
    public decimal DiscountAmount { get; private set; }

    internal static AppliedPromotion Create(Guid quoteId, Guid promotionId, string name, decimal discountAmount)
        => new() { Id = Guid.NewGuid(), QuoteId = quoteId, PromotionId = promotionId, PromotionName = name, DiscountAmount = discountAmount };
}
