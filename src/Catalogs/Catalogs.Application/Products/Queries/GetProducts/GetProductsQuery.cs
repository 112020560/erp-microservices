using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Application.Common;

namespace Catalogs.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    bool? IsActive = null,
    Guid? CategoryId = null,
    Guid? BrandId = null) : IQuery<PagedResult<ProductSummaryResponse>>;
