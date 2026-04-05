using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.ActivateCustomerGroup;

internal sealed class ActivateCustomerGroupCommandHandler(
    ICustomerGroupRepository customerGroupRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateCustomerGroupCommand>
{
    public async Task<Result> Handle(ActivateCustomerGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await customerGroupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (group is null) return Result.Failure(PriceListErrors.CustomerGroupNotFound);

        var result = group.Activate();
        if (result.IsFailure) return result;

        customerGroupRepository.Update(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
