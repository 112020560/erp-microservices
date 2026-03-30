using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Warehouses.Commands.AddLocation;

public sealed record AddLocationCommand(
    Guid WarehouseId,
    string Aisle,
    string Rack,
    string Level,
    string? Name) : ICommand<Guid>;
