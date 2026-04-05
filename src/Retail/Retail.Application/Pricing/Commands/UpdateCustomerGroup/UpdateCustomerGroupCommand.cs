using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.UpdateCustomerGroup;

public sealed record UpdateCustomerGroupCommand(Guid Id, string Name, string? Description) : ICommand;
