using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.CreateProduct;

public sealed record ProductCreatedMessage : IProductCreated
{
    public required Guid ProductId { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required Guid CategoryId { get; init; }
    public required Guid BrandId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
