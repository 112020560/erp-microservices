using Inventory.Domain.Abstractions.Persistence;
using MassTransit;
using SharedKernel.Contracts.Catalogs.Products;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class ProductDeactivatedConsumer(
    IProductSnapshotRepository repository,
    IUnitOfWork unitOfWork) : IConsumer<IProductDeactivated>
{
    public async Task Consume(ConsumeContext<IProductDeactivated> context)
    {
        var msg = context.Message;

        var snapshot = await repository.GetByIdAsync(msg.ProductId, context.CancellationToken);
        if (snapshot is null) return;

        snapshot.SyncFromCatalog(snapshot.Name, snapshot.CategoryId, snapshot.BrandId, false, msg.DeactivatedAt);
        repository.Update(snapshot);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
