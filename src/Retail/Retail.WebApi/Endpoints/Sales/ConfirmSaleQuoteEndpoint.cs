using Asp.Versioning;
using MediatR;
using Retail.Application.Sales.Commands.ConfirmSaleQuote;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class ConfirmSaleQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/sales/quotes/{id:guid}/confirm", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ConfirmSaleQuoteCommand(id), cancellationToken);
            if (result.IsSuccess) return Results.NoContent();
            if (result.Error.Code == "Sale.QuoteNotFound") return Results.NotFound();
            return Results.Problem(result.Error.Description);
        })
        .WithName("ConfirmSaleQuote")
        .WithTags("Sales")
        .WithSummary("Confirmar cotización de venta")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
