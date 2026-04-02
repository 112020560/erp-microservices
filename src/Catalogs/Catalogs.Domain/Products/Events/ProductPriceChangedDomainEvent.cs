using SharedKernel;

namespace Catalogs.Domain.Products.Events;

public sealed record ProductPriceChangedDomainEvent(
    Guid ProductId,
    string Sku,
    decimal OldPrice,
    string OldCurrency,
    decimal NewPrice,
    string NewCurrency,
    DateTimeOffset ChangedAt) : IDomainEvent;
