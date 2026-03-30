using SharedKernel;

namespace Inventory.Domain.PhysicalInventory;

public sealed class CountLine
{
    private CountLine() { }

    public Guid Id { get; private set; }
    public Guid CountId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid? LotId { get; private set; }
    public decimal SystemQuantity { get; private set; }
    public decimal? CountedQuantity { get; private set; }
    public bool IsAdjusted { get; private set; }

    public decimal? Difference => CountedQuantity.HasValue ? CountedQuantity.Value - SystemQuantity : null;

    internal static Result<CountLine> Create(
        Guid countId,
        Guid productId,
        Guid locationId,
        Guid? lotId,
        decimal systemQuantity)
    {
        var line = new CountLine
        {
            Id = Guid.NewGuid(),
            CountId = countId,
            ProductId = productId,
            LocationId = locationId,
            LotId = lotId,
            SystemQuantity = systemQuantity,
            CountedQuantity = null,
            IsAdjusted = false
        };

        return Result.Success(line);
    }

    public Result RecordCount(decimal countedQuantity)
    {
        if (countedQuantity < 0)
            return Result.Failure(Error.Failure("CountLine.InvalidQuantity", "Counted quantity cannot be negative."));

        CountedQuantity = countedQuantity;
        return Result.Success();
    }

    public void MarkAsAdjusted()
    {
        IsAdjusted = true;
    }
}
