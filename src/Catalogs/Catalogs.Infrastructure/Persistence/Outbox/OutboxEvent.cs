namespace Catalogs.Infrastructure.Persistence.Outbox;

public sealed class OutboxEvent
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTimeOffset OccurredOn { get; init; }
    public OutboxEventStatus Status { get; set; } = OutboxEventStatus.Pending;
    public DateTimeOffset? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
