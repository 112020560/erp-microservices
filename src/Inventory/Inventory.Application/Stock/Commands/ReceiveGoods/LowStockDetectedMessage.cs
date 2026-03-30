using SharedKernel.Contracts.Inventory;

namespace Inventory.Application.Stock.Commands.ReceiveGoods;

public sealed record LowStockDetectedMessage : ILowStockDetected
{
    public required Guid ProductId { get; init; }
    public required string Sku { get; init; }
    public required string ProductName { get; init; }
    public required Guid WarehouseId { get; init; }
    public required decimal QuantityOnHand { get; init; }
    public required decimal MinimumStock { get; init; }
    public required DateTimeOffset DetectedAt { get; init; }
}
