using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Products.Commands.DeactivateProduct;

public sealed record DeactivateProductCommand(Guid ProductId) : ICommand;
