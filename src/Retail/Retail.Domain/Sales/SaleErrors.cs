using SharedKernel;
namespace Retail.Domain.Sales;
public static class SaleErrors
{
    public static readonly Error QuoteNotFound = Error.NotFound("Sale.QuoteNotFound", "Sale quote not found.");
    public static readonly Error InvoiceNotFound = Error.NotFound("Sale.InvoiceNotFound", "Sale invoice not found.");
    public static readonly Error QuoteNotOpen = Error.Failure("Sale.QuoteNotOpen", "Quote must be in Open status to invoice.");
    public static readonly Error QuoteAlreadyConfirmed = Error.Failure("Sale.QuoteAlreadyConfirmed", "Quote is already confirmed.");
    public static readonly Error QuoteNotDraft = Error.Failure("Sale.QuoteNotDraft", "Quote must be in Draft status to confirm.");
    public static readonly Error QuoteCancelled = Error.Failure("Sale.QuoteCancelled", "Quote has already been cancelled.");
    public static readonly Error QuoteExpired = Error.Failure("Sale.QuoteExpired", "Quote has expired.");
    public static readonly Error QuoteCannotBeCancelled = Error.Failure("Sale.QuoteCannotBeCancelled", "Only Draft or Open quotes can be cancelled.");
    public static readonly Error NoLines = Error.Failure("Sale.NoLines", "Quote must have at least one line.");
    public static readonly Error InvalidPaymentTotal = Error.Failure("Sale.InvalidPaymentTotal", "Payment total must match the invoice total.");
    public static readonly Error NoPayments = Error.Failure("Sale.NoPayments", "At least one payment is required.");
    public static readonly Error InvalidQuantity = Error.Failure("Sale.InvalidQuantity", "Quantity must be greater than zero.");
    public static readonly Error InvalidPrice = Error.Failure("Sale.InvalidPrice", "Unit price cannot be negative.");
    public static readonly Error InvalidValidUntil = Error.Failure("Sale.InvalidValidUntil", "ValidUntil must be in the future.");
    public static readonly Error CreditRequiresCustomer = Error.Failure("Sale.CreditRequiresCustomer", "A credit payment requires an identified customer.");
    public static readonly Error CustomerNotInCreditSystem = Error.Failure("Sale.CustomerNotInCreditSystem", "Customer is not registered in the credit system.");
    public static readonly Error CreditServiceUnavailable = Error.Failure("Sale.CreditServiceUnavailable", "Credit service is unavailable. Cannot process credit payment.");
}
