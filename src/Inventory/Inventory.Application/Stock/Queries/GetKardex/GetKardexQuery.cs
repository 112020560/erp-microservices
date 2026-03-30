using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Stock.Queries.GetKardex;

public sealed record GetKardexQuery(
    Guid ProductId,
    Guid? WarehouseId,
    DateTimeOffset? From,
    DateTimeOffset? To) : IQuery<IReadOnlyList<KardexEntryResponse>>;
