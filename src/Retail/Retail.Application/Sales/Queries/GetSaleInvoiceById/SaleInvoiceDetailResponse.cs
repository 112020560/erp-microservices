using Retail.Domain.Sales;
namespace Retail.Application.Sales.Queries.GetSaleInvoiceById;

public sealed record SaleInvoiceDetailResponse(
    Guid Id, string InvoiceNumber, Guid QuoteId, Guid CashierId,
    bool RequiresElectronicInvoice, Guid? ElectronicDocumentId,
    decimal Total, IReadOnlyList<PaymentLineResponse> Payments,
    DateTimeOffset CreatedAt);

public sealed record PaymentLineResponse(Guid Id, PaymentMethod Method, decimal Amount, string? Reference);
