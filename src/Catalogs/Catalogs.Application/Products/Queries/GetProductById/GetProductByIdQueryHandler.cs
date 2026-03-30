using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;

namespace Catalogs.Application.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure<ProductResponse>(ProductError.NotFound(request.ProductId));

        return Result.Success(new ProductResponse(
            product.Id,
            product.Sku.Value,
            product.Name,
            product.Description,
            product.Price,
            product.Currency,
            product.CategoryId,
            product.BrandId,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt));
    }
}
