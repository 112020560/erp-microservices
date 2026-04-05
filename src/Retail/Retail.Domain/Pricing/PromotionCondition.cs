namespace Retail.Domain.Pricing;

public sealed class PromotionCondition
{
    private PromotionCondition() { }

    public Guid Id { get; private set; }
    public Guid PromotionId { get; private set; }
    public PromotionConditionType ConditionType { get; private set; }
    public decimal? DecimalValue { get; private set; }  // for Min thresholds
    public Guid? ReferenceId { get; private set; }      // for product/category/group refs
    public int? IntValue { get; private set; }           // for item counts
    public DateTimeOffset CreatedAt { get; private set; }

    internal static PromotionCondition Create(
        Guid promotionId,
        PromotionConditionType conditionType,
        decimal? decimalValue,
        Guid? referenceId,
        int? intValue)
    {
        return new PromotionCondition
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            ConditionType = conditionType,
            DecimalValue = decimalValue,
            ReferenceId = referenceId,
            IntValue = intValue,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
