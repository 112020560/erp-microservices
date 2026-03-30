using SharedKernel;

namespace Inventory.Domain.Stock;

public sealed class StockEntry
{
    private StockEntry() { }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid? LotId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal QuantityReserved { get; private set; }
    public decimal AverageCost { get; private set; }
    public decimal MinimumStock { get; private set; }
    public decimal ReorderPoint { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    public bool IsLowStock => QuantityOnHand <= MinimumStock && MinimumStock > 0;
    public bool NeedsReorder => QuantityOnHand <= ReorderPoint && ReorderPoint > 0;

    public static Result<StockEntry> Create(
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        Guid? lotId,
        decimal minimumStock,
        decimal reorderPoint)
    {
        if (productId == Guid.Empty)
            return Result.Failure<StockEntry>(StockError.ProductRequired);

        if (warehouseId == Guid.Empty)
            return Result.Failure<StockEntry>(StockError.WarehouseRequired);

        if (locationId == Guid.Empty)
            return Result.Failure<StockEntry>(StockError.LocationRequired);

        var entry = new StockEntry
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            LotId = lotId,
            QuantityOnHand = 0,
            QuantityReserved = 0,
            AverageCost = 0,
            MinimumStock = minimumStock,
            ReorderPoint = reorderPoint,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(entry);
    }

    public Result ReceiveStock(decimal quantity, decimal unitCost)
    {
        if (quantity <= 0)
            return Result.Failure(StockError.InvalidQuantity);

        if (unitCost < 0)
            return Result.Failure(StockError.InvalidCost);

        if (QuantityOnHand == 0)
        {
            AverageCost = unitCost;
        }
        else
        {
            AverageCost = (QuantityOnHand * AverageCost + quantity * unitCost) / (QuantityOnHand + quantity);
        }

        QuantityOnHand += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result IssueStock(decimal quantity)
    {
        if (quantity <= 0)
            return Result.Failure(StockError.InvalidQuantity);

        if (QuantityAvailable < quantity)
            return Result.Failure(StockError.InsufficientAvailableStock);

        QuantityOnHand -= quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Reserve(decimal quantity)
    {
        if (quantity <= 0)
            return Result.Failure(StockError.InvalidQuantity);

        if (QuantityAvailable < quantity)
            return Result.Failure(StockError.InsufficientAvailableStock);

        QuantityReserved += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result ReleaseReservation(decimal quantity)
    {
        if (quantity <= 0)
            return Result.Failure(StockError.InvalidQuantity);

        decimal newReserved = QuantityReserved - quantity;
        if (newReserved < 0)
            newReserved = 0;

        QuantityReserved = newReserved;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result AdjustStock(decimal newQuantity, decimal unitCost)
    {
        if (newQuantity < 0)
            return Result.Failure(StockError.InvalidQuantity);

        if (unitCost < 0)
            return Result.Failure(StockError.InvalidCost);

        QuantityOnHand = newQuantity;

        if (newQuantity > 0)
            AverageCost = unitCost;

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
