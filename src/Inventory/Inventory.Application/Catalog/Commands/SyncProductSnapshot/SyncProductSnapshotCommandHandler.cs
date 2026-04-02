using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Catalog;
using SharedKernel;

namespace Inventory.Application.Catalog.Commands.SyncProductSnapshot;

internal sealed class SyncProductSnapshotCommandHandler(
    IProductSnapshotRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<SyncProductSnapshotCommand>
{
    public async Task<Result> Handle(SyncProductSnapshotCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(request.ProductId, cancellationToken);

        if (existing is null)
        {
            var snapshot = ProductSnapshot.CreateFromCatalog(
                request.ProductId, request.Sku, request.Name, request.CategoryId, request.BrandId);
            repository.Add(snapshot);
        }
        else
        {
            existing.SyncFromCatalog(request.Name, request.CategoryId, request.BrandId, true, DateTimeOffset.UtcNow);
            repository.Update(existing);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
