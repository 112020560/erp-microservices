using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Warehouses.Queries.GetWarehouses;

public sealed record GetWarehousesQuery : IQuery<IReadOnlyList<WarehouseResponse>>;
