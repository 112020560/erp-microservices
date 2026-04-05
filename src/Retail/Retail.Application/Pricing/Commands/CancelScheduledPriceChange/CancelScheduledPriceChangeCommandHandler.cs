using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.CancelScheduledPriceChange;

internal sealed class CancelScheduledPriceChangeCommandHandler(
    IScheduledPriceChangeRepository scheduledPriceChangeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelScheduledPriceChangeCommand>
{
    public async Task<Result> Handle(CancelScheduledPriceChangeCommand request, CancellationToken cancellationToken)
    {
        var change = await scheduledPriceChangeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (change is null) return Result.Failure(PriceListErrors.ScheduledPriceChangeNotFound);

        var result = change.Cancel();
        if (result.IsFailure) return result;

        scheduledPriceChangeRepository.Update(change);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
