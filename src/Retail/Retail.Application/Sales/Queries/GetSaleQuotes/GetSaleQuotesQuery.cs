using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Sales;
namespace Retail.Application.Sales.Queries.GetSaleQuotes;
public sealed record GetSaleQuotesQuery(SaleQuoteStatus? Status, Guid? SalesPersonId, Guid? CustomerId)
    : IQuery<IReadOnlyList<SaleQuoteSummaryResponse>>;
