using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.GetPriceListById;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetPriceListByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/price-lists/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPriceListByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        })
        .WithName("GetPriceListById")
        .WithTags("Pricing")
        .Produces<PriceListDetailResponse>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
