using SharedKernel.Contracts.Inventory;

namespace Inventory.Application.Stock.Commands.ReceiveGoods;

public sealed record StockMovementConfirmedMessage : IStockMovementConfirmed
{
    public required Guid MovementId { get; init; }
    public required string MovementNumber { get; init; }
    public required int MovementType { get; init; }
    public required Guid WarehouseId { get; init; }
    public required DateTimeOffset ConfirmedAt { get; init; }
}
