using SharedKernel;

namespace Catalogs.Domain.Products.Events;

public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid BrandId,
    DateTimeOffset CreatedAt) : IDomainEvent;
