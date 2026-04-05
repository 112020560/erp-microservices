using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetScheduledPriceChanges;

internal sealed class GetScheduledPriceChangesQueryHandler(IScheduledPriceChangeRepository scheduledPriceChangeRepository)
    : IQueryHandler<GetScheduledPriceChangesQuery, IReadOnlyList<ScheduledPriceChangeResponse>>
{
    public async Task<Result<IReadOnlyList<ScheduledPriceChangeResponse>>> Handle(
        GetScheduledPriceChangesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ScheduledPriceChange> changes;

        if (request.PriceListId.HasValue)
        {
            changes = await scheduledPriceChangeRepository.GetByPriceListAsync(request.PriceListId.Value, cancellationToken);
            if (request.Status.HasValue)
                changes = changes.Where(c => c.Status == request.Status.Value).ToList().AsReadOnly();
        }
        else
        {
            changes = await scheduledPriceChangeRepository.GetPendingDueAsync(DateTimeOffset.MaxValue, cancellationToken);
            if (request.Status.HasValue)
                changes = changes.Where(c => c.Status == request.Status.Value).ToList().AsReadOnly();
        }

        var response = changes
            .Select(c => new ScheduledPriceChangeResponse(
                c.Id,
                c.PriceListId,
                c.ItemId,
                c.NewPrice,
                c.NewDiscountPercentage,
                c.NewMinPrice,
                c.EffectiveAt,
                c.Status,
                c.AppliedAt,
                c.CancelledAt,
                c.CreatedAt))
            .ToList()
            .AsReadOnly() as IReadOnlyList<ScheduledPriceChangeResponse>;

        return Result.Success(response!);
    }
}
