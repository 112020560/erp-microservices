using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.PhysicalInventory.Commands.ClosePhysicalCount;

public sealed record ClosePhysicalCountCommand(Guid CountId) : ICommand;
