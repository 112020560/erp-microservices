using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing.Abstractions;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;
using SharedKernel;

namespace Retail.Application.Sales.Commands.ConfirmSaleQuote;

internal sealed class ConfirmSaleQuoteCommandHandler(
    ISaleQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ConfirmSaleQuoteCommand>
{
    public async Task<Result> Handle(ConfirmSaleQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null) return Result.Failure(SaleErrors.QuoteNotFound);

        var result = quote.Confirm();
        if (result.IsFailure) return result;

        quoteRepository.Update(quote);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
