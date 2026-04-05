using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.RemoveOrderDiscount;

internal sealed class RemoveOrderDiscountCommandHandler(
    IPriceListRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveOrderDiscountCommand>
{
    public async Task<Result> Handle(RemoveOrderDiscountCommand request, CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdWithItemsAndDiscountsAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure(PriceListErrors.NotFound);

        var result = priceList.RemoveOrderDiscount(request.DiscountId);
        if (result.IsFailure) return result;

        repository.Update(priceList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
