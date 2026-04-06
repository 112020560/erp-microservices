using Retail.Application.Abstractions.Messaging;
namespace Retail.Application.Sales.Commands.CancelSaleQuote;
public sealed record CancelSaleQuoteCommand(Guid QuoteId) : ICommand;
