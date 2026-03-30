using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Application.Common;
using Catalogs.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Catalogs.Application.Products.Queries.GetProducts;

internal sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductSummaryResponse>>
{
    public async Task<Result<PagedResult<ProductSummaryResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await productRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.IsActive,
            request.CategoryId,
            request.BrandId,
            cancellationToken);

        var responses = items
            .Select(p => new ProductSummaryResponse(
                p.Id,
                p.Sku.Value,
                p.Name,
                p.Price,
                p.Currency,
                p.CategoryId,
                p.BrandId,
                p.IsActive,
                p.UpdatedAt))
            .ToList();

        return Result.Success(new PagedResult<ProductSummaryResponse>(
            responses,
            totalCount,
            request.Page,
            request.PageSize));
    }
}
