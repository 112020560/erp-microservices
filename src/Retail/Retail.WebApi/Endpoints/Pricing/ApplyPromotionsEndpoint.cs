using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.ApplyPromotions;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class ApplyPromotionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/apply-promotions", async (
            ApplyPromotionsRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var cartItems = request.Items
                .Select(i => new CartLineItem(i.ProductId, i.CategoryId, i.Quantity, i.LineTotal))
                .ToArray();

            var query = new ApplyPromotionsQuery(
                request.CustomerId,
                request.CustomerGroupIds,
                request.Channel,
                cartItems,
                request.Subtotal,
                request.TotalQuantity,
                request.CouponCode);

            var result = await mediator.Send(query, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description);
        })
        .WithName("ApplyPromotions")
        .WithSummary("Evaluar y aplicar promociones al carrito")
        .WithTags("Pricing")
        .Produces<PromotionsApplicationResult>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record ApplyPromotionsRequest(
    Guid? CustomerId,
    Guid[] CustomerGroupIds,
    string Channel,
    CartLineItemRequest[] Items,
    decimal Subtotal,
    decimal TotalQuantity,
    string? CouponCode);

public sealed record CartLineItemRequest(
    Guid ProductId,
    Guid? CategoryId,
    decimal Quantity,
    decimal LineTotal);
