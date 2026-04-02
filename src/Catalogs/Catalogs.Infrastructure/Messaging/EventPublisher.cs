using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Infrastructure.Persistence;
using MassTransit;

namespace Catalogs.Infrastructure.Messaging;

// internal sealed class EventPublisher(
//     IDbContextOutbox<CatalogsDbContext> outbox,
//     CatalogsDbContext dbContext) : IEventPublisher
// {
//     public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
//     {
//         outbox.Initialize(null, dbContext);
//         return outbox.Publish(message, cancellationToken);
//     }
// }
