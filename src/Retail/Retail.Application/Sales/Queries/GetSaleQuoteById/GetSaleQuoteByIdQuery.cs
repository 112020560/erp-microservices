using Retail.Application.Abstractions.Messaging;
namespace Retail.Application.Sales.Queries.GetSaleQuoteById;
public sealed record GetSaleQuoteByIdQuery(Guid Id) : IQuery<SaleQuoteDetailResponse>;
