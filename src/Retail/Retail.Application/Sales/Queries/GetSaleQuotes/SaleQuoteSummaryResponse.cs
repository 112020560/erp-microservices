using Retail.Domain.Sales;
namespace Retail.Application.Sales.Queries.GetSaleQuotes;
public sealed record SaleQuoteSummaryResponse(
    Guid Id, string QuoteNumber, Guid SalesPersonId,
    Guid? CustomerId, string CustomerName,
    SaleQuoteStatus Status, string Currency,
    decimal Total, DateTimeOffset ValidUntil,
    int LineCount, DateTimeOffset CreatedAt);
