using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.CreatePriceList;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class CreatePriceListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/price-lists", async (
            CreatePriceListRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePriceListCommand(
                request.Name,
                request.Currency,
                request.Priority,
                request.RoundingRule,
                request.ValidFrom,
                request.ValidTo);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/price-lists/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("CreatePriceList")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record CreatePriceListRequest(
    string Name,
    string Currency,
    int Priority,
    RoundingRule RoundingRule,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo);
