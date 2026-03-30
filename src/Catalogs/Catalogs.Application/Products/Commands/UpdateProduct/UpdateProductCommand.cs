using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid BrandId) : ICommand;
