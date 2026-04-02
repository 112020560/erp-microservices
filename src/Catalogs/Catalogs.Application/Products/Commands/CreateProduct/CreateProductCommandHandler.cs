using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;

namespace Catalogs.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (await productRepository.ExistsBySkuAsync(request.Sku, cancellationToken))
            return Result.Failure<Guid>(ProductError.SkuAlreadyExists(request.Sku));

        var result = Product.Create(
            request.Sku,
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.CategoryId,
            request.BrandId);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var product = result.Value;

        productRepository.Add(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
