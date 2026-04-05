using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.SetVolumeTiers;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class SetVolumeTiersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/pricing/price-lists/{id:guid}/volume-tiers", async (
            Guid id,
            SetVolumeTiersRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var tiers = request.Tiers
                .Select(t => new VolumeTierRequest(
                    t.MinQuantity, t.MaxQuantity, t.Price,
                    t.DiscountPercentage, t.MinPrice, t.PriceIncludesTax))
                .ToList()
                .AsReadOnly();

            var command = new SetVolumeTiersCommand(id, request.ItemType, request.ReferenceId, tiers);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Results.NoContent() : Results.Problem(result.Error.Description);
        })
        .WithName("SetVolumeTiers")
        .WithTags("Pricing")
        .WithSummary("Reemplazar todos los niveles de precio por volumen para un producto o categoría")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record SetVolumeTiersRequest(
    PriceItemType ItemType,
    Guid? ReferenceId,
    IReadOnlyList<VolumeTierItemRequest> Tiers);

public sealed record VolumeTierItemRequest(
    decimal MinQuantity,
    decimal? MaxQuantity,
    decimal Price,
    decimal DiscountPercentage,
    decimal? MinPrice,
    bool PriceIncludesTax);
