using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.RemovePriceListItem;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class RemovePriceListItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/pricing/price-lists/{id:guid}/items/{itemId:guid}", async (
            Guid id,
            Guid itemId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new RemovePriceListItemCommand(id, itemId), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : Results.Problem(result.Error.Description);
        })
        .WithName("RemovePriceListItem")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
