using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Reservations.Commands.ReleaseStockReservation;

public sealed record ReleaseStockReservationCommand(Guid ReservationId) : ICommand;
