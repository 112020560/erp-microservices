using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;
using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductError.NotFound(request.ProductId));

        var result = product.Update(request.Name, request.Description, request.CategoryId, request.BrandId);

        if (result.IsFailure)
            return result;

        productRepository.Update(product);

        await eventPublisher.PublishAsync(new ProductUpdatedMessage
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            UpdatedAt = product.UpdatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
