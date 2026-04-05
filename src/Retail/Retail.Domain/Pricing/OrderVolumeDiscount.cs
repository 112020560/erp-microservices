namespace Retail.Domain.Pricing;

public sealed class OrderVolumeDiscount
{
    private OrderVolumeDiscount() { }

    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public decimal? MinOrderTotal { get; private set; }
    public decimal? MinOrderQuantity { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    internal static OrderVolumeDiscount Create(
        Guid priceListId,
        decimal? minOrderTotal,
        decimal? minOrderQuantity,
        decimal discountPercentage,
        decimal? discountAmount,
        decimal? maxDiscountAmount,
        int priority)
    {
        return new OrderVolumeDiscount
        {
            Id = Guid.NewGuid(),
            PriceListId = priceListId,
            MinOrderTotal = minOrderTotal,
            MinOrderQuantity = minOrderQuantity,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            MaxDiscountAmount = maxDiscountAmount,
            Priority = priority,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public bool AppliesTo(decimal subtotal, decimal totalQuantity)
    {
        if (!IsActive) return false;
        if (MinOrderTotal.HasValue && subtotal < MinOrderTotal.Value) return false;
        if (MinOrderQuantity.HasValue && totalQuantity < MinOrderQuantity.Value) return false;
        return true;
    }

    public decimal CalculateDiscount(decimal subtotal)
    {
        var discount = DiscountAmount ?? (subtotal * DiscountPercentage / 100);
        if (MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, MaxDiscountAmount.Value);
        return discount;
    }
}
