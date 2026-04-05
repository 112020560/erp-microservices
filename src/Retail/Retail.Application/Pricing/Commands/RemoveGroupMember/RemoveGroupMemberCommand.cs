using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.RemoveGroupMember;

public sealed record RemoveGroupMemberCommand(Guid GroupId, Guid CustomerId) : ICommand;
