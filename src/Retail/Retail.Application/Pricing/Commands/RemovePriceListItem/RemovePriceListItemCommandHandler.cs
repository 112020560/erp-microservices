using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.RemovePriceListItem;

internal sealed class RemovePriceListItemCommandHandler(
    IPriceListRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemovePriceListItemCommand>
{
    public async Task<Result> Handle(RemovePriceListItemCommand request, CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdWithItemsAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure(PriceListErrors.NotFound);

        var result = priceList.RemoveItem(request.ItemId);
        if (result.IsFailure) return result;

        repository.Update(priceList);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
