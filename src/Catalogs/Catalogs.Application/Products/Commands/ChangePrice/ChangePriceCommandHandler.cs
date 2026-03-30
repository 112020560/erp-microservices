using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;
using SharedKernel.Contracts.Catalogs.Products;

namespace Catalogs.Application.Products.Commands.ChangePrice;

internal sealed class ChangePriceCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : ICommandHandler<ChangePriceCommand>
{
    public async Task<Result> Handle(ChangePriceCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductError.NotFound(request.ProductId));

        decimal oldPrice = product.Price;
        string oldCurrency = product.Currency;

        var result = product.ChangePrice(request.NewPrice, request.Currency);

        if (result.IsFailure)
            return result;

        productRepository.Update(product);

        await eventPublisher.PublishAsync(new ProductPriceChangedMessage
        {
            ProductId = product.Id,
            Sku = product.Sku.Value,
            OldPrice = oldPrice,
            OldCurrency = oldCurrency,
            NewPrice = product.Price,
            NewCurrency = product.Currency,
            ChangedAt = product.UpdatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
