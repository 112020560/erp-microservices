using SharedKernel;

namespace Inventory.Domain.Lots;

public sealed class Lot
{
    private Lot() { }

    public Guid Id { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public DateOnly? ManufacturingDate { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    public static Result<Lot> Create(
        string lotNumber,
        Guid productId,
        DateOnly? manufacturingDate,
        DateOnly? expirationDate)
    {
        if (string.IsNullOrWhiteSpace(lotNumber))
            return Result.Failure<Lot>(LotError.LotNumberRequired);

        if (productId == Guid.Empty)
            return Result.Failure<Lot>(LotError.ProductRequired);

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            ProductId = productId,
            ManufacturingDate = manufacturingDate,
            ExpirationDate = expirationDate,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(lot);
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(LotError.AlreadyInactive);

        IsActive = false;
        return Result.Success();
    }
}
