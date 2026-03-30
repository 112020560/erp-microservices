using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Inventory.Application.Warehouses.Queries.GetWarehouses;

internal sealed class GetWarehousesQueryHandler(
    IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetWarehousesQuery, IReadOnlyList<WarehouseResponse>>
{
    public async Task<Result<IReadOnlyList<WarehouseResponse>>> Handle(
        GetWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        var warehouses = await warehouseRepository.GetAllActiveAsync(cancellationToken);

        var response = warehouses
            .Select(w => new WarehouseResponse(
                w.Id,
                w.Code,
                w.Name,
                w.Description,
                w.IsActive,
                w.Locations
                    .Select(l => new LocationResponse(
                        l.Id,
                        l.Aisle,
                        l.Rack,
                        l.Level,
                        l.Name,
                        l.Code,
                        l.IsActive))
                    .ToList()
                    .AsReadOnly()))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<WarehouseResponse>>(response);
    }
}
