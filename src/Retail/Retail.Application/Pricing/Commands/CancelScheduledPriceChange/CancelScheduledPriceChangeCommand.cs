using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.CancelScheduledPriceChange;

public sealed record CancelScheduledPriceChangeCommand(Guid Id) : ICommand;
