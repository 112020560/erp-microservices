using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.RemoveGroupMember;

internal sealed class RemoveGroupMemberCommandHandler(
    ICustomerGroupRepository customerGroupRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveGroupMemberCommand>
{
    public async Task<Result> Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await customerGroupRepository.GetByIdWithMembersAsync(request.GroupId, cancellationToken);
        if (group is null) return Result.Failure(PriceListErrors.CustomerGroupNotFound);

        var result = group.RemoveMember(request.CustomerId);
        if (result.IsFailure) return result;

        customerGroupRepository.Update(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
