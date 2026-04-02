namespace Catalogs.Infrastructure.Persistence.Outbox;

public enum OutboxEventStatus
{
    Pending = 0,
    Delivered = 1,
    Failed = 2
}
