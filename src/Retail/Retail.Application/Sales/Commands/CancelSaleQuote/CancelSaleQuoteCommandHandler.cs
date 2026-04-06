using MassTransit;
using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing.Abstractions;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;
using SharedKernel;
using SharedKernel.Contracts.Sales;

namespace Retail.Application.Sales.Commands.CancelSaleQuote;

internal sealed class CancelSaleQuoteCommandHandler(
    ISaleQuoteRepository quoteRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelSaleQuoteCommand>
{
    public async Task<Result> Handle(CancelSaleQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null) return Result.Failure(SaleErrors.QuoteNotFound);

        var result = quote.Cancel();
        if (result.IsFailure) return result;

        quoteRepository.Update(quote);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new SaleQuoteCancelledEvent
        {
            QuoteId = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            CancelledAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        return Result.Success();
    }
}
