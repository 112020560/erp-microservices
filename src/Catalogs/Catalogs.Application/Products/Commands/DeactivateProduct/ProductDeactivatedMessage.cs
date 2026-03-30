using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.DeactivateProduct;

public sealed record ProductDeactivatedMessage : IProductDeactivated
{
    public required Guid ProductId { get; init; }
    public required string Sku { get; init; }
    public required DateTimeOffset DeactivatedAt { get; init; }
}
