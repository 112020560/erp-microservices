using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AddPriceListItem;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AddPriceListItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/price-lists/{id:guid}/items", async (
            Guid id,
            AddPriceListItemRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AddPriceListItemCommand(
                id,
                request.ItemType,
                request.ReferenceId,
                request.MinQuantity,
                request.MaxQuantity,
                request.Price,
                request.DiscountPercentage,
                request.MinPrice,
                request.PriceIncludesTax);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/price-lists/{id}/items/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AddPriceListItem")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AddPriceListItemRequest(
    PriceItemType ItemType,
    Guid? ReferenceId,
    decimal MinQuantity,
    decimal? MaxQuantity,
    decimal Price,
    decimal DiscountPercentage,
    decimal? MinPrice,
    bool PriceIncludesTax);
