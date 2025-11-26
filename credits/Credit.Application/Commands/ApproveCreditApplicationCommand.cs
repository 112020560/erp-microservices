using Credit.Application.Abstractions.Messaging;
using SharedKernel;

namespace Credit.Application.Commands;

public record ApproveCreditApplicationCommand(
        Guid CreditApplicationId
    ) : ICommand<Guid>;

internal sealed class ApproveCreditApplicationHandler: ICommandHandler<ApproveCreditApplicationCommand, Guid>
{
    public Task<Result<Guid>> Handle(ApproveCreditApplicationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}