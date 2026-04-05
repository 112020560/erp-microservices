using SharedKernel;

namespace Retail.Domain.Pricing;

public sealed class CustomerGroupPriceList
{
    private CustomerGroupPriceList() { }

    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid PriceListId { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<CustomerGroupPriceList> Create(
        Guid groupId,
        Guid priceListId,
        int priority,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null)
    {
        if (validFrom.HasValue && validTo.HasValue && validTo <= validFrom)
            return Result.Failure<CustomerGroupPriceList>(
                Error.Failure("CustomerGroupPriceList.InvalidDateRange", "ValidTo must be after ValidFrom."));

        return Result.Success(new CustomerGroupPriceList
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
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
