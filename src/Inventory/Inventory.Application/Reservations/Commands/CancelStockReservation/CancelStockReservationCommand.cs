using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Reservations.Commands.CancelStockReservation;

public sealed record CancelStockReservationCommand(Guid ReservationId) : ICommand;
