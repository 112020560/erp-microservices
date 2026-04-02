using SharedKernel;

namespace Catalogs.Domain.Products.Events;

public sealed record ProductUpdatedDomainEvent(
    Guid ProductId,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid BrandId,
    DateTimeOffset UpdatedAt) : IDomainEvent;
