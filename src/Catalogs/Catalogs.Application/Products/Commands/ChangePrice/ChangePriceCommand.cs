using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Products.Commands.ChangePrice;

public sealed record ChangePriceCommand(
    Guid ProductId,
    decimal NewPrice,
    string Currency) : ICommand;
