using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.GetScheduledPriceChanges;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetScheduledPriceChangesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/scheduled-changes", async (
            Guid? priceListId,
            ScheduledPriceChangeStatus? status,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetScheduledPriceChangesQuery(priceListId, status);
            var result = await mediator.Send(query, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description);
        })
        .WithName("GetScheduledPriceChanges")
        .WithTags("Pricing")
        .Produces<IReadOnlyList<ScheduledPriceChangeResponse>>(200)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
