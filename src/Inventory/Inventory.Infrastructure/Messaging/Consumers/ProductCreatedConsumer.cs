using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Catalog;
using MassTransit;
using SharedKernel.Contracts.Catalogs.Products;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class ProductCreatedConsumer(
    IProductSnapshotRepository repository,
    IUnitOfWork unitOfWork) : IConsumer<IProductCreated>
{
    public async Task Consume(ConsumeContext<IProductCreated> context)
    {
        var msg = context.Message;

        var existing = await repository.GetByIdAsync(msg.ProductId, context.CancellationToken);
        if (existing is not null) return; // idempotent

        var snapshot = ProductSnapshot.CreateFromCatalog(
            msg.ProductId, msg.Sku, msg.Name, msg.CategoryId, msg.BrandId);

        repository.Add(snapshot);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
