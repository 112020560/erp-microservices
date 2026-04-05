using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.AddGroupMember;

public sealed record AddGroupMemberCommand(Guid GroupId, Guid CustomerId) : ICommand<Guid>;
