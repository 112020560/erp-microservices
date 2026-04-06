using Asp.Versioning;
using MediatR;
using Retail.Application.Sales.Queries.GetSaleQuoteById;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class GetSaleQuoteByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales/quotes/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetSaleQuoteByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        })
        .WithName("GetSaleQuoteById")
        .WithTags("Sales")
        .WithSummary("Obtener cotización de venta por ID")
        .Produces<SaleQuoteDetailResponse>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
