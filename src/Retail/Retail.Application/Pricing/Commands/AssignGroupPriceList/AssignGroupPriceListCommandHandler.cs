using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AssignGroupPriceList;

internal sealed class AssignGroupPriceListCommandHandler(
    ICustomerGroupRepository customerGroupRepository,
    IPriceListRepository priceListRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignGroupPriceListCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AssignGroupPriceListCommand request, CancellationToken cancellationToken)
    {
        var group = await customerGroupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null) return Result.Failure<Guid>(PriceListErrors.CustomerGroupNotFound);

        var priceList = await priceListRepository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure<Guid>(PriceListErrors.NotFound);

        var result = CustomerGroupPriceList.Create(
            request.GroupId,
            request.PriceListId,
            request.Priority,
            request.ValidFrom,
            request.ValidTo);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await customerGroupRepository.AddGroupPriceListAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
