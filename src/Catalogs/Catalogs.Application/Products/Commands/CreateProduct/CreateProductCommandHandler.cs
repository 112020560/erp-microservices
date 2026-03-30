using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;
using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
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

        await eventPublisher.PublishAsync(new ProductCreatedMessage
        {
            ProductId = product.Id,
            Sku = product.Sku.Value,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Currency = product.Currency,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            CreatedAt = product.CreatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
