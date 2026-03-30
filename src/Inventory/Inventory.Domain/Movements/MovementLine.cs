using SharedKernel;

namespace Inventory.Domain.Movements;

public sealed class MovementLine
{
    private MovementLine() { }

    public Guid Id { get; private set; }
    public Guid MovementId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid SourceLocationId { get; private set; }
    public Guid? DestinationLocationId { get; private set; }
    public Guid? LotId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? Notes { get; private set; }

    internal static Result<MovementLine> Create(
        Guid movementId,
        Guid productId,
        Guid sourceLocationId,
        Guid? destinationLocationId,
        Guid? lotId,
        decimal quantity,
        decimal unitCost,
        string? notes)
    {
        if (quantity <= 0)
            return Result.Failure<MovementLine>(
                Error.Failure("MovementLine.InvalidQuantity", "Quantity must be greater than zero."));

        if (unitCost < 0)
            return Result.Failure<MovementLine>(
                Error.Failure("MovementLine.InvalidCost", "Unit cost cannot be negative."));

        var line = new MovementLine
        {
            Id = Guid.NewGuid(),
            MovementId = movementId,
            ProductId = productId,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            LotId = lotId,
            Quantity = quantity,
            UnitCost = unitCost,
            Notes = notes?.Trim()
        };

        return Result.Success(line);
    }
}
