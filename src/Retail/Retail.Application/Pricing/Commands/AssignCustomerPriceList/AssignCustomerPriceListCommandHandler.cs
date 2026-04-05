using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AssignCustomerPriceList;

internal sealed class AssignCustomerPriceListCommandHandler(
    IPriceListRepository priceListRepository,
    ICustomerPriceListRepository customerRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignCustomerPriceListCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AssignCustomerPriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = await priceListRepository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure<Guid>(PriceListErrors.NotFound);

        var result = CustomerPriceList.Create(request.CustomerId, request.PriceListId, request.ValidFrom, request.ValidTo);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await customerRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
