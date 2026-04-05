using SharedKernel;

namespace Retail.Domain.Pricing;

public sealed class CustomerPriceList
{
    private CustomerPriceList() { }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid PriceListId { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<CustomerPriceList> Create(
        Guid customerId,
        Guid priceListId,
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo)
    {
        if (validFrom.HasValue && validTo.HasValue && validTo <= validFrom)
            return Result.Failure<CustomerPriceList>(Error.Failure(
                "CustomerPriceList.InvalidDateRange", "ValidTo must be after ValidFrom."));

        return Result.Success(new CustomerPriceList
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PriceListId = priceListId,
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
