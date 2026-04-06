using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Sales.Abstractions;
using SharedKernel;

namespace Retail.Application.Sales.Queries.GetSaleQuotes;

internal sealed class GetSaleQuotesQueryHandler(ISaleQuoteRepository quoteRepository)
    : IQueryHandler<GetSaleQuotesQuery, IReadOnlyList<SaleQuoteSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<SaleQuoteSummaryResponse>>> Handle(
        GetSaleQuotesQuery request,
        CancellationToken cancellationToken)
    {
        var quotes = await quoteRepository.GetAllAsync(request.Status, request.SalesPersonId, request.CustomerId, cancellationToken);

        var response = quotes
            .Select(q => new SaleQuoteSummaryResponse(
                q.Id,
                q.QuoteNumber,
                q.SalesPersonId,
                q.CustomerId,
                q.CustomerName,
                q.Status,
                q.Currency,
                q.Total,
                q.ValidUntil,
                q.Lines.Count,
                q.CreatedAt))
            .ToList()
            .AsReadOnly() as IReadOnlyList<SaleQuoteSummaryResponse>;

        return Result.Success(response!);
    }
}
