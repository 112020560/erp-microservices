using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Products.Commands.ActivateProduct;

public sealed record ActivateProductCommand(Guid ProductId) : ICommand;
