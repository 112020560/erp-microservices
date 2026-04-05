using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.CreateCustomerGroup;

internal sealed class CreateCustomerGroupCommandHandler(
    ICustomerGroupRepository customerGroupRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCustomerGroupCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCustomerGroupCommand request, CancellationToken cancellationToken)
    {
        var result = CustomerGroup.Create(request.Name, request.Description);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await customerGroupRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
