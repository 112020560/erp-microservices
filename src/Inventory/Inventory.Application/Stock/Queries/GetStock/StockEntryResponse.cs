namespace Inventory.Application.Stock.Queries.GetStock;

public sealed record StockEntryResponse(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    Guid LocationId,
    Guid? LotId,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal AverageCost,
    decimal MinimumStock,
    bool IsLowStock);
