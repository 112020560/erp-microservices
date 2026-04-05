using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Retail.Application.Pricing.Queries.GetPriceLists;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetPriceListsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/price-lists", async (
            [FromQuery] bool? isActive,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPriceListsQuery(isActive), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description);
        })
        .WithName("GetPriceLists")
        .WithTags("Pricing")
        .Produces<IReadOnlyList<PriceListSummaryResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
