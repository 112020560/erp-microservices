using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.PhysicalInventory.Commands.StartPhysicalCount;

public sealed record StartPhysicalCountCommand(
    Guid WarehouseId,
    string? Notes,
    IReadOnlyList<CountLineInitDto> Lines) : ICommand<string>;

public sealed record CountLineInitDto(
    Guid ProductId,
    Guid LocationId,
    Guid? LotId);
