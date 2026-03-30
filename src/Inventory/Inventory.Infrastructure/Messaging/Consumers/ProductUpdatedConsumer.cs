using Inventory.Domain.Abstractions.Persistence;
using MassTransit;
using SharedKernel.Contracts.Catalogs.Products;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class ProductUpdatedConsumer(
    IProductSnapshotRepository repository,
    IUnitOfWork unitOfWork) : IConsumer<IProductUpdated>
{
    public async Task Consume(ConsumeContext<IProductUpdated> context)
    {
        var msg = context.Message;

        var snapshot = await repository.GetByIdAsync(msg.ProductId, context.CancellationToken);
        if (snapshot is null) return;

        snapshot.SyncFromCatalog(msg.Name, msg.CategoryId, msg.BrandId, true, msg.UpdatedAt);
        repository.Update(snapshot);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
