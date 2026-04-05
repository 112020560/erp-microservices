namespace Retail.Domain.Pricing;

public sealed class PriceListItem
{
    private PriceListItem() { }

    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public PriceItemType ItemType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public decimal MinQuantity { get; private set; }
    public decimal? MaxQuantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal? MinPrice { get; private set; }
    public bool PriceIncludesTax { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    internal static PriceListItem Create(
        Guid priceListId,
        PriceItemType itemType,
        Guid? referenceId,
        decimal minQuantity,
        decimal? maxQuantity,
        decimal price,
        decimal discountPercentage,
        decimal? minPrice,
        bool priceIncludesTax)
    {
        return new PriceListItem
        {
            Id = Guid.NewGuid(),
            PriceListId = priceListId,
            ItemType = itemType,
            ReferenceId = referenceId,
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            Price = price,
            DiscountPercentage = discountPercentage,
            MinPrice = minPrice,
            PriceIncludesTax = priceIncludesTax,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    internal void Update(decimal price, decimal discountPercentage, decimal? minPrice, bool priceIncludesTax)
    {
        Price = price;
        DiscountPercentage = discountPercentage;
        MinPrice = minPrice;
        PriceIncludesTax = priceIncludesTax;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void ApplyScheduledChange(decimal price, decimal? discountPercentage, decimal? minPrice)
    {
        if (price >= 0) Price = price;
        if (discountPercentage.HasValue) DiscountPercentage = discountPercentage.Value;
        if (minPrice.HasValue) MinPrice = minPrice;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
