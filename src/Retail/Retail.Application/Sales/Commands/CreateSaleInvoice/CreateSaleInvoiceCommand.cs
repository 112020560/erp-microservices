using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Sales;

namespace Retail.Application.Sales.Commands.CreateSaleInvoice;

public sealed record CreateSaleInvoiceCommand(
    Guid QuoteId,
    Guid CashierId,
    bool RequiresElectronicInvoice,
    Guid? TenantId,
    IReadOnlyList<CreatePaymentLineDto> Payments) : ICommand<Guid>;

public sealed record CreatePaymentLineDto(PaymentMethod Method, decimal Amount, string? Reference);
