using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Lots.Commands.CreateLot;

public sealed record CreateLotCommand(
    string LotNumber,
    Guid ProductId,
    DateOnly? ManufacturingDate,
    DateOnly? ExpirationDate) : ICommand<Guid>;
