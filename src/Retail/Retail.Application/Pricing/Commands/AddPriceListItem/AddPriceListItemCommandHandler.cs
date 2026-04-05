using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AddPriceListItem;

internal sealed class AddPriceListItemCommandHandler(
    IPriceListRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddPriceListItemCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddPriceListItemCommand request, CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdWithItemsAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure<Guid>(PriceListErrors.NotFound);

        var result = priceList.AddItem(
            request.ItemType,
            request.ReferenceId,
            request.MinQuantity,
            request.MaxQuantity,
            request.Price,
            request.DiscountPercentage,
            request.MinPrice,
            request.PriceIncludesTax);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        repository.Update(priceList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
