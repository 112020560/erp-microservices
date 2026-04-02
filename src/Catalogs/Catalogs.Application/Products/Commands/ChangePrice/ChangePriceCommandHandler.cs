using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using SharedKernel;

namespace Catalogs.Application.Products.Commands.ChangePrice;

internal sealed class ChangePriceCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangePriceCommand>
{
    public async Task<Result> Handle(ChangePriceCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductError.NotFound(request.ProductId));

        var result = product.ChangePrice(request.NewPrice, request.Currency);

        if (result.IsFailure)
            return result;

        productRepository.Update(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
