using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.ActivateCustomerGroup;

public sealed record ActivateCustomerGroupCommand(Guid Id) : ICommand;
