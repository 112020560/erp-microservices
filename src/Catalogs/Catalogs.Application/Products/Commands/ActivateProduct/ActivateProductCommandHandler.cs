using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;
using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.ActivateProduct;

internal sealed class ActivateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : ICommandHandler<ActivateProductCommand>
{
    public async Task<Result> Handle(ActivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductError.NotFound(request.ProductId));

        var result = product.Activate();

        if (result.IsFailure)
            return result;

        productRepository.Update(product);

        await eventPublisher.PublishAsync(new ProductActivatedMessage
        {
            ProductId = product.Id,
            Sku = product.Sku.Value,
            ActivatedAt = product.UpdatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
