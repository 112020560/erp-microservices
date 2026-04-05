using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.RemoveOrderDiscount;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class RemoveOrderDiscountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/pricing/price-lists/{id:guid}/order-discounts/{discountId:guid}", async (
            Guid id,
            Guid discountId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveOrderDiscountCommand(id, discountId);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Results.NoContent() : Results.Problem(result.Error.Description);
        })
        .WithName("RemoveOrderDiscount")
        .WithTags("Pricing")
        .WithSummary("Eliminar descuento por volumen de pedido de una lista de precios")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
