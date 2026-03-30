namespace Catalogs.Application.Products.Queries.GetProducts;

public sealed record ProductSummaryResponse(
    Guid Id,
    string Sku,
    string Name,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid BrandId,
    bool IsActive,
    DateTimeOffset UpdatedAt);
