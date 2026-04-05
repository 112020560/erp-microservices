using SharedKernel;

namespace Retail.Domain.Pricing;

public sealed class ChannelPriceList
{
    private ChannelPriceList() { }

    public Guid Id { get; private set; }
    public SalesChannel Channel { get; private set; }
    public Guid PriceListId { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<ChannelPriceList> Create(
        SalesChannel channel,
        Guid priceListId,
        int priority,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null)
    {
        if (validFrom.HasValue && validTo.HasValue && validTo <= validFrom)
            return Result.Failure<ChannelPriceList>(
                Error.Failure("ChannelPriceList.InvalidDateRange", "ValidTo must be after ValidFrom."));

        return Result.Success(new ChannelPriceList
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            PriceListId = priceListId,
            Priority = priority,
            ValidFrom = validFrom,
            ValidTo = validTo,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public bool IsValidAt(DateTimeOffset date)
    {
        if (ValidFrom.HasValue && date < ValidFrom.Value) return false;
        if (ValidTo.HasValue && date > ValidTo.Value) return false;
        return true;
    }
}
