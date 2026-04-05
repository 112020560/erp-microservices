using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Stock.Queries.SearchPosStock;

public sealed record SearchPosStockQuery(
    string? Q,
    string? Sku,
    Guid? WarehouseId,
    Guid? CategoryId,
    bool OnlyAvailable,
    int Page,
    int PageSize) : IQuery<PosStockPagedResponse>;
