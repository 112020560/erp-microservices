namespace Retail.Domain.Pricing;

public sealed record PriceResolutionContext(
    Guid? CustomerId,
    SalesChannel Channel,
    Guid ProductId,
    Guid CategoryId,
    decimal Quantity,
    string Currency,
    DateTimeOffset? At = null);
