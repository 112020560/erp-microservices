using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;
using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.DeactivateProduct;

internal sealed class DeactivateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : ICommandHandler<DeactivateProductCommand>
{
    public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductError.NotFound(request.ProductId));

        var result = product.Deactivate();

        if (result.IsFailure)
            return result;

        productRepository.Update(product);

        await eventPublisher.PublishAsync(new ProductDeactivatedMessage
        {
            ProductId = product.Id,
            Sku = product.Sku.Value,
            DeactivatedAt = product.UpdatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
