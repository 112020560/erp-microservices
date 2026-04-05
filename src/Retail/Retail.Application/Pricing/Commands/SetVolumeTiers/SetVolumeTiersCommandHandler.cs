using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.SetVolumeTiers;

internal sealed class SetVolumeTiersCommandHandler(
    IPriceListRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetVolumeTiersCommand>
{
    public async Task<Result> Handle(SetVolumeTiersCommand request, CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdWithItemsAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure(PriceListErrors.NotFound);

        var result = priceList.SetVolumeTiers(request.ItemType, request.ReferenceId, request.Tiers);
        if (result.IsFailure) return result;

        repository.Update(priceList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
