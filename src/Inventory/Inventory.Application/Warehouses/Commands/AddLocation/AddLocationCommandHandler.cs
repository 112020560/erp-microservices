using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Warehouses;
using SharedKernel;

namespace Inventory.Application.Warehouses.Commands.AddLocation;

internal sealed class AddLocationCommandHandler(
    IWarehouseRepository warehouseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddLocationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddLocationCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
            return Result.Failure<Guid>(WarehouseError.NotFound(request.WarehouseId));

        var result = warehouse.AddLocationAndReturn(request.Aisle, request.Rack, request.Level, request.Name);
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        warehouseRepository.Update(warehouse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
