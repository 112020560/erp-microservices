using SharedKernel;

namespace Catalogs.Domain.Products.Events;

public sealed record ProductActivatedDomainEvent(
    Guid ProductId,
    string Sku,
    DateTimeOffset ActivatedAt) : IDomainEvent;
