using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.PhysicalInventory.Commands.RecordPhysicalCount;

public sealed record RecordPhysicalCountCommand(
    Guid CountId,
    Guid LineId,
    decimal CountedQuantity) : ICommand;
