using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Commands.SetVolumeTiers;

public sealed record SetVolumeTiersCommand(
    Guid PriceListId,
    PriceItemType ItemType,
    Guid? ReferenceId,
    IReadOnlyList<VolumeTierRequest> Tiers) : ICommand;
