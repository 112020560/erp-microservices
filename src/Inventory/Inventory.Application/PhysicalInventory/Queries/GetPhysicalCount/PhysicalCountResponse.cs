namespace Inventory.Application.PhysicalInventory.Queries.GetPhysicalCount;

public sealed record PhysicalCountResponse(
    Guid Id,
    string CountNumber,
    Guid WarehouseId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<CountLineResponse> Lines);

public sealed record CountLineResponse(
    Guid Id,
    Guid ProductId,
    Guid LocationId,
    Guid? LotId,
    decimal SystemQuantity,
    decimal? CountedQuantity,
    decimal? Difference,
    bool IsAdjusted);
