using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Brands.Queries.GetBrands;

public sealed record GetBrandsQuery : IQuery<IReadOnlyList<BrandResponse>>;
