using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.SchedulePriceChange;

internal sealed class SchedulePriceChangeCommandHandler(
    IPriceListRepository priceListRepository,
    IScheduledPriceChangeRepository scheduledPriceChangeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SchedulePriceChangeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SchedulePriceChangeCommand request, CancellationToken cancellationToken)
    {
        var priceList = await priceListRepository.GetByIdWithItemsAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure<Guid>(PriceListErrors.NotFound);

        var item = priceList.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null) return Result.Failure<Guid>(PriceListErrors.ItemNotFound(request.ItemId));

        var result = ScheduledPriceChange.Create(
            request.PriceListId,
            request.ItemId,
            request.NewPrice,
            request.NewDiscountPercentage,
            request.NewMinPrice,
            request.EffectiveAt);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await scheduledPriceChangeRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
