namespace Inventory.Application.Warehouses.Queries.GetWarehouses;

public sealed record WarehouseResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<LocationResponse> Locations);

public sealed record LocationResponse(
    Guid Id,
    string Aisle,
    string Rack,
    string Level,
    string? Name,
    string Code,
    bool IsActive);
