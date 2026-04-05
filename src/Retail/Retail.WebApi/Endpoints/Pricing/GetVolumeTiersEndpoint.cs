using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.GetVolumeTiers;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetVolumeTiersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/price-lists/{id:guid}/volume-tiers", async (
            Guid id,
            PriceItemType itemType,
            Guid? referenceId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetVolumeTiersQuery(id, itemType, referenceId);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description);
        })
        .WithName("GetVolumeTiers")
        .WithTags("Pricing")
        .WithSummary("Obtener niveles de precio por volumen para un producto o categoría")
        .Produces<IReadOnlyList<VolumeTierResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
