using Asp.Versioning;
using MediatR;
using Retail.Application.Sales.Commands.CancelSaleQuote;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class CancelSaleQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/sales/quotes/{id:guid}/cancel", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new CancelSaleQuoteCommand(id), cancellationToken);
            if (result.IsSuccess) return Results.NoContent();
            if (result.Error.Code == "Sale.QuoteNotFound") return Results.NotFound();
            return Results.Problem(result.Error.Description);
        })
        .WithName("CancelSaleQuote")
        .WithTags("Sales")
        .WithSummary("Cancelar cotización de venta")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
