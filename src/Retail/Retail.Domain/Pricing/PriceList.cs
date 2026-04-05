using SharedKernel;

namespace Retail.Domain.Pricing;

public sealed class PriceList
{
    private readonly List<PriceListItem> _items = [];
    private readonly List<OrderVolumeDiscount> _orderDiscounts = [];

    private PriceList() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public RoundingRule RoundingRule { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<PriceListItem> Items => _items.AsReadOnly();
    public IReadOnlyList<OrderVolumeDiscount> OrderDiscounts => _orderDiscounts.AsReadOnly();

    public static Result<PriceList> Create(
        string name,
        string currency,
        int priority,
        RoundingRule roundingRule,
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<PriceList>(PriceListErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result.Failure<PriceList>(PriceListErrors.InvalidCurrency);

        if (validFrom.HasValue && validTo.HasValue && validTo <= validFrom)
            return Result.Failure<PriceList>(PriceListErrors.InvalidDateRange);

        return Result.Success(new PriceList
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Currency = currency.ToUpperInvariant(),
            Priority = priority,
            IsActive = true,
            RoundingRule = roundingRule,
            ValidFrom = validFrom,
            ValidTo = validTo,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    public Result Update(string name, int priority, RoundingRule roundingRule, DateTimeOffset? validFrom, DateTimeOffset? validTo)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(PriceListErrors.NameRequired);

        if (validFrom.HasValue && validTo.HasValue && validTo <= validFrom)
            return Result.Failure(PriceListErrors.InvalidDateRange);

        Name = name.Trim();
        Priority = priority;
        RoundingRule = roundingRule;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive) return Result.Failure(PriceListErrors.AlreadyActive);
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive) return Result.Failure(PriceListErrors.AlreadyInactive);
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result<PriceListItem> AddItem(
        PriceItemType itemType,
        Guid? referenceId,
        decimal minQuantity,
        decimal? maxQuantity,
        decimal price,
        decimal discountPercentage,
        decimal? minPrice,
        bool priceIncludesTax)
    {
        if (price < 0)
            return Result.Failure<PriceListItem>(PriceListErrors.InvalidPrice);

        if (discountPercentage < 0 || discountPercentage > 100)
            return Result.Failure<PriceListItem>(PriceListErrors.InvalidDiscount);

        if (minQuantity < 0)
            return Result.Failure<PriceListItem>(PriceListErrors.InvalidQuantity);

        if (maxQuantity.HasValue && maxQuantity <= minQuantity)
            return Result.Failure<PriceListItem>(PriceListErrors.InvalidQuantityRange);

        if (itemType != PriceItemType.Default && !referenceId.HasValue)
            return Result.Failure<PriceListItem>(PriceListErrors.ReferenceIdRequired);

        // Check overlapping ranges for same type+reference
        if (_items.Any(i =>
            i.ItemType == itemType &&
            i.ReferenceId == referenceId &&
            QuantityRangesOverlap(i.MinQuantity, i.MaxQuantity, minQuantity, maxQuantity)))
            return Result.Failure<PriceListItem>(PriceListErrors.OverlappingQuantityRange);

        var item = PriceListItem.Create(
            Id, itemType, referenceId, minQuantity, maxQuantity,
            price, discountPercentage, minPrice, priceIncludesTax);

        _items.Add(item);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(item);
    }

    public Result RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return Result.Failure(PriceListErrors.ItemNotFound(itemId));
        _items.Remove(item);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    private static bool QuantityRangesOverlap(decimal aMin, decimal? aMax, decimal bMin, decimal? bMax)
    {
        var aEnd = aMax ?? decimal.MaxValue;
        var bEnd = bMax ?? decimal.MaxValue;
        return aMin < bEnd && bMin < aEnd;
    }

    public Result SetVolumeTiers(
        PriceItemType itemType,
        Guid? referenceId,
        IReadOnlyList<VolumeTierRequest> tiers)
    {
        if (itemType != PriceItemType.Default && !referenceId.HasValue)
            return Result.Failure(PriceListErrors.ReferenceIdRequired);

        // Validate no overlaps within the new tiers
        for (int i = 0; i < tiers.Count; i++)
        {
            for (int j = i + 1; j < tiers.Count; j++)
            {
                if (QuantityRangesOverlap(tiers[i].MinQuantity, tiers[i].MaxQuantity,
                                          tiers[j].MinQuantity, tiers[j].MaxQuantity))
                    return Result.Failure(PriceListErrors.OverlappingQuantityRange);
            }
        }

        // Validate each tier
        foreach (var tier in tiers)
        {
            if (tier.Price < 0) return Result.Failure(PriceListErrors.InvalidPrice);
            if (tier.DiscountPercentage < 0 || tier.DiscountPercentage > 100) return Result.Failure(PriceListErrors.InvalidDiscount);
            if (tier.MinQuantity < 0) return Result.Failure(PriceListErrors.InvalidQuantity);
            if (tier.MaxQuantity.HasValue && tier.MaxQuantity <= tier.MinQuantity) return Result.Failure(PriceListErrors.InvalidQuantityRange);
        }

        // Remove existing items for this type+reference
        _items.RemoveAll(i => i.ItemType == itemType && i.ReferenceId == referenceId);

        // Add new tiers
        foreach (var tier in tiers)
        {
            _items.Add(PriceListItem.Create(Id, itemType, referenceId,
                tier.MinQuantity, tier.MaxQuantity, tier.Price,
                tier.DiscountPercentage, tier.MinPrice, tier.PriceIncludesTax));
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result<OrderVolumeDiscount> AddOrderDiscount(
        decimal? minOrderTotal,
        decimal? minOrderQuantity,
        decimal discountPercentage,
        decimal? discountAmount,
        decimal? maxDiscountAmount,
        int priority)
    {
        if (!minOrderTotal.HasValue && !minOrderQuantity.HasValue)
            return Result.Failure<OrderVolumeDiscount>(PriceListErrors.OrderDiscountInvalidThreshold);

        if (discountPercentage <= 0 && (!discountAmount.HasValue || discountAmount <= 0))
            return Result.Failure<OrderVolumeDiscount>(PriceListErrors.OrderDiscountInvalidDiscount);

        if (discountPercentage < 0 || discountPercentage > 100)
            return Result.Failure<OrderVolumeDiscount>(PriceListErrors.OrderDiscountInvalidPercentage);

        var discount = OrderVolumeDiscount.Create(
            Id, minOrderTotal, minOrderQuantity,
            discountPercentage, discountAmount, maxDiscountAmount, priority);

        _orderDiscounts.Add(discount);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(discount);
    }

    public Result RemoveOrderDiscount(Guid discountId)
    {
        var discount = _orderDiscounts.FirstOrDefault(d => d.Id == discountId);
        if (discount is null) return Result.Failure(PriceListErrors.OrderDiscountNotFound(discountId));
        _orderDiscounts.Remove(discount);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public bool IsValidAt(DateTimeOffset date)
    {
        if (!IsActive) return false;
        if (ValidFrom.HasValue && date < ValidFrom.Value) return false;
        if (ValidTo.HasValue && date > ValidTo.Value) return false;
        return true;
    }

    public Result ApplyScheduledItemChange(Guid itemId, decimal price, decimal? discountPercentage, decimal? minPrice)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return Result.Failure(PriceListErrors.ItemNotFound(itemId));
        item.ApplyScheduledChange(price, discountPercentage, minPrice);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
