using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AddPromotionCondition;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AddPromotionConditionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/promotions/{id:guid}/conditions", async (
            Guid id,
            AddPromotionConditionRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AddPromotionConditionCommand(
                id,
                request.ConditionType,
                request.DecimalValue,
                request.ReferenceId,
                request.IntValue);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/promotions/{id}/conditions/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AddPromotionCondition")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AddPromotionConditionRequest(
    PromotionConditionType ConditionType,
    decimal? DecimalValue,
    Guid? ReferenceId,
    int? IntValue);
