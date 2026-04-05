using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.GetPromotionById;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetPromotionByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/promotions/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPromotionByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description);
        })
        .WithName("GetPromotionById")
        .WithTags("Pricing")
        .Produces<PromotionDetailResponse>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
