using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AssignGroupPriceList;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AssignGroupPriceListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/customer-groups/{id:guid}/price-lists", async (
            Guid id,
            AssignGroupPriceListRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AssignGroupPriceListCommand(
                id,
                request.PriceListId,
                request.Priority,
                request.ValidFrom,
                request.ValidTo);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/customer-groups/{id}/price-lists/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AssignGroupPriceList")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AssignGroupPriceListRequest(
    Guid PriceListId,
    int Priority,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo);
