using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel;
using System.Text.Json;

namespace Catalogs.Infrastructure.Persistence.Outbox;

public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is CatalogsDbContext context)
            InsertOutboxEvents(context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void InsertOutboxEvents(CatalogsDbContext context)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (aggregates.Count == 0)
            return;

        var outboxEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .Select(e => new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = e.GetType().Name,
                Payload = JsonSerializer.Serialize(e, e.GetType()),
                OccurredOn = DateTimeOffset.UtcNow
            })
            .ToList();

        context.OutboxEvents.AddRange(outboxEvents);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }
}
