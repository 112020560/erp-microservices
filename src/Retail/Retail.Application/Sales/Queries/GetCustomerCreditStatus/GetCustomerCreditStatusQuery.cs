using Retail.Application.Abstractions.Messaging;
using Retail.Application.Abstractions.Services;
using SharedKernel;

namespace Retail.Application.Sales.Queries.GetCustomerCreditStatus;

public sealed record GetCustomerCreditStatusQuery(Guid CustomerId)
    : IQuery<CustomerCreditStatusDto>;

internal sealed class GetCustomerCreditStatusQueryHandler(ICreditServiceClient creditClient)
    : IQueryHandler<GetCustomerCreditStatusQuery, CustomerCreditStatusDto>
{
    public async Task<Result<CustomerCreditStatusDto>> Handle(
        GetCustomerCreditStatusQuery request, CancellationToken cancellationToken)
    {
        var status = await creditClient.GetCustomerCreditStatusAsync(request.CustomerId, cancellationToken);

        if (status is null)
            return Result.Failure<CustomerCreditStatusDto>(
                Error.Failure("Credit.ServiceUnavailable", "Credit service is unavailable."));

        return Result.Success(status);
    }
}
