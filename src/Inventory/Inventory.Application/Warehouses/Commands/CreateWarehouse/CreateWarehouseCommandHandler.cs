using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Warehouses;
using SharedKernel;

namespace Inventory.Application.Warehouses.Commands.CreateWarehouse;

internal sealed class CreateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateWarehouseCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        if (await warehouseRepository.ExistsByCodeAsync(request.Code, cancellationToken))
            return Result.Failure<Guid>(WarehouseError.CodeAlreadyExists(request.Code));

        var result = Warehouse.Create(request.Code, request.Name, request.Description);
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var warehouse = result.Value;
        warehouseRepository.Add(warehouse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(warehouse.Id);
    }
}
