using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Stock.Commands.TransferStock;

public sealed record TransferStockCommand(
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string? Reference,
    string? Notes,
    DateTimeOffset Date,
    IReadOnlyList<TransferLineDto> Lines) : ICommand<string>;

public sealed record TransferLineDto(
    Guid ProductId,
    Guid SourceLocationId,
    Guid DestinationLocationId,
    Guid? LotId,
    decimal Quantity);
