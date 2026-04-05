using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.ActivatePromotion;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class ActivatePromotionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/pricing/promotions/{id:guid}/activate", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new ActivatePromotionCommand(id);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description);
        })
        .WithName("ActivatePromotion")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
