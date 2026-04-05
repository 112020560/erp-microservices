using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AddPromotionAction;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AddPromotionActionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/promotions/{id:guid}/actions", async (
            Guid id,
            AddPromotionActionRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AddPromotionActionCommand(
                id,
                request.ActionType,
                request.DiscountPercentage,
                request.DiscountAmount,
                request.TargetReferenceId,
                request.BuyQuantity,
                request.GetQuantity,
                request.BuyReferenceId,
                request.GetReferenceId);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/promotions/{id}/actions/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AddPromotionAction")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AddPromotionActionRequest(
    PromotionActionType ActionType,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    Guid? TargetReferenceId,
    int? BuyQuantity,
    int? GetQuantity,
    Guid? BuyReferenceId,
    Guid? GetReferenceId);
