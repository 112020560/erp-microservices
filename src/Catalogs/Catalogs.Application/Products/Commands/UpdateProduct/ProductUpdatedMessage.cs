using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.UpdateProduct;

public sealed record ProductUpdatedMessage : IProductUpdated
{
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Guid CategoryId { get; init; }
    public required Guid BrandId { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
