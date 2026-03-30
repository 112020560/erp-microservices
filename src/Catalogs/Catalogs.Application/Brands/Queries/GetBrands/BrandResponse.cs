namespace Catalogs.Application.Brands.Queries.GetBrands;

public sealed record BrandResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive);
