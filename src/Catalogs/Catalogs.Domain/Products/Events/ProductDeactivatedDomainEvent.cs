using SharedKernel;

namespace Catalogs.Domain.Products.Events;

public sealed record ProductDeactivatedDomainEvent(
    Guid ProductId,
    string Sku,
    DateTimeOffset DeactivatedAt) : IDomainEvent;
