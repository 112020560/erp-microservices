using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.CreateCustomerGroup;

public sealed record CreateCustomerGroupCommand(string Name, string? Description) : ICommand<Guid>;
