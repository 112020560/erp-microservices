using Retail.Application.Abstractions.Messaging;
namespace Retail.Application.Sales.Queries.GetSaleInvoiceById;
public sealed record GetSaleInvoiceByIdQuery(Guid Id) : IQuery<SaleInvoiceDetailResponse>;
