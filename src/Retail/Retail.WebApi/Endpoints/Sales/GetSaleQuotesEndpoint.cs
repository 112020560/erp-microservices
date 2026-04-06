using Asp.Versioning;
using MediatR;
using Retail.Application.Sales.Queries.GetSaleQuotes;
using Retail.Domain.Sales;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class GetSaleQuotesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales/quotes", async (
            SaleQuoteStatus? status,
            Guid? salesPersonId,
            Guid? customerId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new GetSaleQuotesQuery(status, salesPersonId, customerId),
                cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description);
        })
        .WithName("GetSaleQuotes")
        .WithTags("Sales")
        .WithSummary("Listar cotizaciones de venta")
        .Produces<IReadOnlyList<SaleQuoteSummaryResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
