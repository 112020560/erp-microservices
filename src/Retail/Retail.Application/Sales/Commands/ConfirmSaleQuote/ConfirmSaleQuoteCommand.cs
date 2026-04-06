using Retail.Application.Abstractions.Messaging;
namespace Retail.Application.Sales.Commands.ConfirmSaleQuote;
public sealed record ConfirmSaleQuoteCommand(Guid QuoteId) : ICommand;
