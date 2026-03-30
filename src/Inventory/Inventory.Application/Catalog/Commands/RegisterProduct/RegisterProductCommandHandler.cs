using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Inventory.Application.Catalog.Commands.RegisterProduct;

internal sealed class RegisterProductCommandHandler(
    IProductSnapshotRepository productSnapshotRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterProductCommand>
{
    public async Task<Result> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
    {
        var snapshot = await productSnapshotRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (snapshot is null)
            return Result.Failure(Error.NotFound("Product.NotFound", $"Product '{request.ProductId}' not found in inventory."));

        var result = snapshot.ConfigureInventory(request.TrackingType, request.MinimumStock, request.ReorderPoint);
        if (result.IsFailure)
            return result;

        productSnapshotRepository.Update(snapshot);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
