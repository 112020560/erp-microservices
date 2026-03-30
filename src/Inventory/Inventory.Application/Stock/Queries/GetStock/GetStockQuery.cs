using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Stock.Queries.GetStock;

public sealed record GetStockQuery(
    Guid? ProductId,
    Guid? WarehouseId,
    bool? IsLowStock) : IQuery<IReadOnlyList<StockEntryResponse>>;
