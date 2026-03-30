using SharedKernel;

namespace Inventory.Domain.Movements;

public sealed class InventoryMovement
{
    private readonly List<MovementLine> _lines = [];

    private InventoryMovement() { }

    public Guid Id { get; private set; }
    public string MovementNumber { get; private set; } = string.Empty;
    public MovementType MovementType { get; private set; }
    public MovementStatus Status { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? DestinationWarehouseId { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<MovementLine> Lines => _lines.AsReadOnly();

    public static Result<InventoryMovement> Create(
        string movementNumber,
        MovementType type,
        Guid warehouseId,
        Guid? destinationWarehouseId,
        string? reference,
        string? notes,
        DateTimeOffset date)
    {
        if (string.IsNullOrWhiteSpace(movementNumber))
            return Result.Failure<InventoryMovement>(MovementError.MovementNumberRequired);

        if (warehouseId == Guid.Empty)
            return Result.Failure<InventoryMovement>(MovementError.WarehouseRequired);

        var movement = new InventoryMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = movementNumber,
            MovementType = type,
            Status = MovementStatus.Draft,
            WarehouseId = warehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Reference = reference?.Trim(),
            Notes = notes?.Trim(),
            Date = date,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(movement);
    }

    public Result AddLine(
        Guid productId,
        Guid sourceLocationId,
        Guid? destinationLocationId,
        Guid? lotId,
        decimal quantity,
        decimal unitCost,
        string? notes)
    {
        if (Status != MovementStatus.Draft)
            return Result.Failure(MovementError.CannotModifyConfirmed);

        var result = MovementLine.Create(Id, productId, sourceLocationId, destinationLocationId, lotId, quantity, unitCost, notes);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        _lines.Add(result.Value);
        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status == MovementStatus.Confirmed)
            return Result.Failure(MovementError.AlreadyConfirmed);

        if (Status == MovementStatus.Cancelled)
            return Result.Failure(MovementError.AlreadyCancelled);

        if (_lines.Count == 0)
            return Result.Failure(MovementError.NoLines);

        Status = MovementStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == MovementStatus.Confirmed)
            return Result.Failure(MovementError.AlreadyConfirmed);

        if (Status == MovementStatus.Cancelled)
            return Result.Failure(MovementError.AlreadyCancelled);

        Status = MovementStatus.Cancelled;
        return Result.Success();
    }

    public Result<MovementLine> GetLine(Guid lineId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        return line is not null
            ? Result.Success(line)
            : Result.Failure<MovementLine>(MovementError.NotFound(lineId));
    }
}
