using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.GetVolumeTiers;

public sealed record GetVolumeTiersQuery(
    Guid PriceListId,
    PriceItemType ItemType,
    Guid? ReferenceId) : IQuery<IReadOnlyList<VolumeTierResponse>>;
