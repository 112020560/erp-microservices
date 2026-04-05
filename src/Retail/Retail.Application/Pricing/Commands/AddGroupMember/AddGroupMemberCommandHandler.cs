using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AddGroupMember;

internal sealed class AddGroupMemberCommandHandler(
    ICustomerGroupRepository customerGroupRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddGroupMemberCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await customerGroupRepository.GetByIdWithMembersAsync(request.GroupId, cancellationToken);
        if (group is null) return Result.Failure<Guid>(PriceListErrors.CustomerGroupNotFound);

        var result = group.AddMember(request.CustomerId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        customerGroupRepository.Update(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
