using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Stock.Commands.AdjustStock;

public sealed record AdjustStockCommand(
    Guid ProductId,
    Guid WarehouseId,
    Guid LocationId,
    Guid? LotId,
    decimal NewQuantity,
    decimal UnitCost,
    string? Reference,
    string? Notes) : ICommand<string>;
