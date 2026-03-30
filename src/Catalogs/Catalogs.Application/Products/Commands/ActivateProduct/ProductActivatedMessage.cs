using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.ActivateProduct;

public sealed record ProductActivatedMessage : IProductActivated
{
    public required Guid ProductId { get; init; }
    public required string Sku { get; init; }
    public required DateTimeOffset ActivatedAt { get; init; }
}
