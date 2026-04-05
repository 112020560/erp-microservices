using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AssignChannelPriceList;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AssignChannelPriceListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/channel-assignments", async (
            AssignChannelRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AssignChannelPriceListCommand(request.Channel, request.PriceListId, request.Priority);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/channel-assignments/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AssignChannelPriceList")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AssignChannelRequest(SalesChannel Channel, Guid PriceListId, int Priority);
