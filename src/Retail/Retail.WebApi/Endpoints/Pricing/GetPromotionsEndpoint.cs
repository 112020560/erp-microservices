using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Retail.Application.Pricing.Queries.GetPromotions;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetPromotionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/promotions", async (
            [FromQuery] bool? isActive,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPromotionsQuery(isActive), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description);
        })
        .WithName("GetPromotions")
        .WithTags("Pricing")
        .Produces<IReadOnlyList<PromotionSummaryResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
