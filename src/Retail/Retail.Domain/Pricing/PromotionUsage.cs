namespace Retail.Domain.Pricing;

public sealed class PromotionUsage
{
    private PromotionUsage() { }

    public Guid Id { get; private set; }
    public Guid PromotionId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string? ExternalOrderId { get; private set; }
    public DateTimeOffset UsedAt { get; private set; }

    internal static PromotionUsage Create(Guid promotionId, Guid? customerId, string? externalOrderId)
    {
        return new PromotionUsage
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            CustomerId = customerId,
            ExternalOrderId = externalOrderId,
            UsedAt = DateTimeOffset.UtcNow
        };
    }
}
