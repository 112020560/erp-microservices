using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Reservations.Commands.CreateStockReservation;

public sealed record CreateStockReservationCommand(
    Guid ProductId,
    Guid WarehouseId,
    Guid LocationId,
    Guid? LotId,
    decimal Quantity,
    Guid SalesOrderId,
    DateTimeOffset? ExpiresAt) : ICommand<Guid>;
