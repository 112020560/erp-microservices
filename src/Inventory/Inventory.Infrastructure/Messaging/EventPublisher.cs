using Inventory.Application.Abstractions.Messaging;
using MassTransit;

namespace Inventory.Infrastructure.Messaging;

internal sealed class EventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class =>
        publishEndpoint.Publish(message, cancellationToken);
}
