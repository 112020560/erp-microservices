using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.DeactivateCustomerGroup;

public sealed record DeactivateCustomerGroupCommand(Guid Id) : ICommand;
