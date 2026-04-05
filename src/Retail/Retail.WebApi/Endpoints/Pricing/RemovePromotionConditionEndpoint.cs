using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.RemovePromotionCondition;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class RemovePromotionConditionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/pricing/promotions/{id:guid}/conditions/{conditionId:guid}", async (
            Guid id,
            Guid conditionId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RemovePromotionConditionCommand(id, conditionId);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description);
        })
        .WithName("RemovePromotionCondition")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
