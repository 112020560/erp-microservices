using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Stock.Commands.ReceiveGoods;

public sealed record ReceiveGoodsCommand(
    Guid WarehouseId,
    string? Reference,
    string? Notes,
    DateTimeOffset Date,
    IReadOnlyList<ReceiveGoodsLineDto> Lines) : ICommand<string>;

public sealed record ReceiveGoodsLineDto(
    Guid ProductId,
    Guid LocationId,
    Guid? LotId,
    decimal Quantity,
    decimal UnitCost);
