using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Stock.Commands.IssueGoods;

public sealed record IssueGoodsCommand(
    Guid WarehouseId,
    string? Reference,
    string? Notes,
    DateTimeOffset Date,
    IReadOnlyList<IssueGoodsLineDto> Lines) : ICommand<string>;

public sealed record IssueGoodsLineDto(
    Guid ProductId,
    Guid LocationId,
    Guid? LotId,
    decimal Quantity);
