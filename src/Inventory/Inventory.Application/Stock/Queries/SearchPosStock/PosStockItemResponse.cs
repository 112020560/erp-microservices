namespace Inventory.Application.Stock.Queries.SearchPosStock;

public sealed record PosStockPagedResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<PosStockItemResponse> Items);

public sealed record PosStockItemResponse(
    Guid ProductId,
    string Sku,
    string Name,
    Guid CategoryId,
    decimal TotalAvailable,
    decimal TotalReserved,
    bool IsLowStock,
    IReadOnlyList<PosWarehouseStockResponse> Warehouses);

public sealed record PosWarehouseStockResponse(
    Guid WarehouseId,
    string WarehouseName,
    decimal Available,
    decimal Reserved);
