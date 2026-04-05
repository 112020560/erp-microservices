using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.GetScheduledPriceChanges;

public sealed record GetScheduledPriceChangesQuery(
    Guid? PriceListId,
    ScheduledPriceChangeStatus? Status) : IQuery<IReadOnlyList<ScheduledPriceChangeResponse>>;
