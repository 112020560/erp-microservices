using SharedKernel;

namespace Retail.Domain.Pricing;

public sealed class ScheduledPriceChange
{
    private ScheduledPriceChange() { }

    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal NewPrice { get; private set; }
    public decimal? NewDiscountPercentage { get; private set; }
    public decimal? NewMinPrice { get; private set; }
    public DateTimeOffset EffectiveAt { get; private set; }
    public ScheduledPriceChangeStatus Status { get; private set; }
    public DateTimeOffset? AppliedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<ScheduledPriceChange> Create(
        Guid priceListId,
        Guid itemId,
        decimal newPrice,
        decimal? newDiscountPercentage,
        decimal? newMinPrice,
        DateTimeOffset effectiveAt)
    {
        if (effectiveAt <= DateTimeOffset.UtcNow)
            return Result.Failure<ScheduledPriceChange>(PriceListErrors.ScheduledPriceChangeInvalidEffectiveAt);

        if (newPrice < 0)
            return Result.Failure<ScheduledPriceChange>(PriceListErrors.InvalidPrice);

        if (newDiscountPercentage.HasValue && (newDiscountPercentage < 0 || newDiscountPercentage > 100))
            return Result.Failure<ScheduledPriceChange>(PriceListErrors.InvalidDiscount);

        return Result.Success(new ScheduledPriceChange
        {
            Id = Guid.NewGuid(),
            PriceListId = priceListId,
            ItemId = itemId,
            NewPrice = newPrice,
            NewDiscountPercentage = newDiscountPercentage,
            NewMinPrice = newMinPrice,
            EffectiveAt = effectiveAt,
            Status = ScheduledPriceChangeStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public Result Apply()
    {
        if (Status == ScheduledPriceChangeStatus.Applied)
            return Result.Failure(PriceListErrors.ScheduledPriceChangeAlreadyApplied);
        if (Status == ScheduledPriceChangeStatus.Cancelled)
            return Result.Failure(PriceListErrors.ScheduledPriceChangeAlreadyCancelled);

        Status = ScheduledPriceChangeStatus.Applied;
        AppliedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == ScheduledPriceChangeStatus.Applied)
            return Result.Failure(PriceListErrors.ScheduledPriceChangeAlreadyApplied);
        if (Status == ScheduledPriceChangeStatus.Cancelled)
            return Result.Failure(PriceListErrors.ScheduledPriceChangeAlreadyCancelled);

        Status = ScheduledPriceChangeStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
