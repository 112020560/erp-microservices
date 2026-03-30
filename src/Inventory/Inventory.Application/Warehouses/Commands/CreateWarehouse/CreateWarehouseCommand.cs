using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Warehouses.Commands.CreateWarehouse;

public sealed record CreateWarehouseCommand(
    string Code,
    string Name,
    string? Description) : ICommand<Guid>;
