using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.CreatePromotion;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class CreatePromotionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/promotions", async (
            CreatePromotionRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePromotionCommand(
                request.Name,
                request.Description,
                request.CouponCode,
                request.ValidFrom,
                request.ValidTo,
                request.MaxUses,
                request.MaxUsesPerCustomer,
                request.Priority,
                request.CanStackWithOthers);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/promotions/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("CreatePromotion")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record CreatePromotionRequest(
    string Name,
    string? Description,
    string? CouponCode,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    int? MaxUses,
    int? MaxUsesPerCustomer,
    int Priority,
    bool CanStackWithOthers);
