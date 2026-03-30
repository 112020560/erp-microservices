using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.ChangePrice;

public sealed record ProductPriceChangedMessage : IProductPriceChanged
{
    public required Guid ProductId { get; init; }
    public required string Sku { get; init; }
    public required decimal OldPrice { get; init; }
    public required string OldCurrency { get; init; }
    public required decimal NewPrice { get; init; }
    public required string NewCurrency { get; init; }
    public required DateTimeOffset ChangedAt { get; init; }
}
