using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid BrandId) : ICommand<Guid>;
