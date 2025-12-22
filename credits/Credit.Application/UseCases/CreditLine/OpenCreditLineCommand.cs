

using Credit.Application.Abstractions.Messaging;
using SharedKernel;

namespace Credit.Application.Commands;

public record OpenCreditLineCommand(
    Guid ApplicationId,
    Guid CustomerId,
    Guid ProductId,
    decimal Principal,
    decimal Outstanding,
    string Currency,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    string? AmortizationSchedule,
    string? Metadata
): ICommand<Guid>;


internal sealed class OpenCreditLineHandler : ICommandHandler<OpenCreditLineCommand, Guid>
{
    public Task<Result<Guid>> Handle(OpenCreditLineCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}