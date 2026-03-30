using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.PhysicalInventory;
using SharedKernel;

namespace Inventory.Application.PhysicalInventory.Queries.GetPhysicalCount;

internal sealed class GetPhysicalCountQueryHandler(
    IPhysicalCountRepository physicalCountRepository)
    : IQueryHandler<GetPhysicalCountQuery, PhysicalCountResponse>
{
    public async Task<Result<PhysicalCountResponse>> Handle(
        GetPhysicalCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await physicalCountRepository.GetByIdAsync(request.CountId, cancellationToken);
        if (count is null)
            return Result.Failure<PhysicalCountResponse>(PhysicalCountError.NotFound(request.CountId));

        var response = new PhysicalCountResponse(
            count.Id,
            count.CountNumber,
            count.WarehouseId,
            count.Status.ToString(),
            count.StartedAt,
            count.CompletedAt,
            count.Lines
                .Select(l => new CountLineResponse(
                    l.Id,
                    l.ProductId,
                    l.LocationId,
                    l.LotId,
                    l.SystemQuantity,
                    l.CountedQuantity,
                    l.Difference,
                    l.IsAdjusted))
                .ToList()
                .AsReadOnly());

        return Result.Success(response);
    }
}
