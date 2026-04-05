using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.RemovePromotionAction;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class RemovePromotionActionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/pricing/promotions/{id:guid}/actions/{actionId:guid}", async (
            Guid id,
            Guid actionId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RemovePromotionActionCommand(id, actionId);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description);
        })
        .WithName("RemovePromotionAction")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
