using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.UpdateCustomerGroup;

internal sealed class UpdateCustomerGroupCommandHandler(
    ICustomerGroupRepository customerGroupRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCustomerGroupCommand>
{
    public async Task<Result> Handle(UpdateCustomerGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await customerGroupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (group is null) return Result.Failure(PriceListErrors.CustomerGroupNotFound);

        var result = group.Update(request.Name, request.Description);
        if (result.IsFailure) return result;

        customerGroupRepository.Update(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
