using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.UpdatePromotion;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class UpdatePromotionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/pricing/promotions/{id:guid}", async (
            Guid id,
            UpdatePromotionRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePromotionCommand(
                id,
                request.Name,
                request.Description,
                request.ValidFrom,
                request.ValidTo,
                request.MaxUses,
                request.MaxUsesPerCustomer,
                request.Priority,
                request.CanStackWithOthers);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description);
        })
        .WithName("UpdatePromotion")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record UpdatePromotionRequest(
    string Name,
    string? Description,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    int? MaxUses,
    int? MaxUsesPerCustomer,
    int Priority,
    bool CanStackWithOthers);
