using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.SchedulePriceChange;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class CreateScheduledPriceChangeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/scheduled-changes", async (
            CreateScheduledPriceChangeRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new SchedulePriceChangeCommand(
                request.PriceListId,
                request.ItemId,
                request.NewPrice,
                request.NewDiscountPercentage,
                request.NewMinPrice,
                request.EffectiveAt);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/scheduled-changes/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("CreateScheduledPriceChange")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record CreateScheduledPriceChangeRequest(
    Guid PriceListId,
    Guid ItemId,
    decimal NewPrice,
    decimal? NewDiscountPercentage,
    decimal? NewMinPrice,
    DateTimeOffset EffectiveAt);
