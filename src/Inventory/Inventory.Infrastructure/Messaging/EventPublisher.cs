// using Inventory.Application.Abstractions.Messaging;
// using Inventory.Infrastructure.Persistence;
// using MassTransit;
//
// namespace Inventory.Infrastructure.Messaging;
//
// internal sealed class EventPublisher(
//     IDbContextOutbox<InventoryDbContext> outbox,
//     InventoryDbContext dbContext) : IEventPublisher
// {
//     public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
//     {
//         outbox.Initialize(null, dbContext);
//         return outbox.Publish(message, cancellationToken);
//     }
// }
