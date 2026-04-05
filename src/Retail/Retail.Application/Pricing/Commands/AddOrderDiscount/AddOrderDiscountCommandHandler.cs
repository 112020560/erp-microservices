using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AddOrderDiscount;

internal sealed class AddOrderDiscountCommandHandler(
    IPriceListRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddOrderDiscountCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddOrderDiscountCommand request, CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdWithItemsAndDiscountsAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure<Guid>(PriceListErrors.NotFound);

        var result = priceList.AddOrderDiscount(
            request.MinOrderTotal,
            request.MinOrderQuantity,
            request.DiscountPercentage,
            request.DiscountAmount,
            request.MaxDiscountAmount,
            request.Priority);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        repository.Update(priceList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
