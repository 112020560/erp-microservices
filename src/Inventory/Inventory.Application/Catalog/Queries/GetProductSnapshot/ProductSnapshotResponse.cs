namespace Inventory.Application.Catalog.Queries.GetProductSnapshot;

public sealed record ProductSnapshotResponse(
    Guid ProductId,
    string Sku,
    string Name,
    string TrackingType,
    decimal MinimumStock,
    decimal ReorderPoint,
    bool IsActive);
