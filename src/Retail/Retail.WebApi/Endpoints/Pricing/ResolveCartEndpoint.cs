using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.ResolveCart;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class ResolveCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/resolve-cart", async (
            ResolveCartRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var items = request.Items
                .Select(i => new CartItemRequest(i.ProductId, i.CategoryId, i.Quantity))
                .ToList()
                .AsReadOnly();

            var query = new ResolveCartQuery(request.CustomerId, request.Channel, request.Currency, items);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description);
        })
        .WithName("ResolveCart")
        .WithTags("Pricing")
        .WithSummary("Resolver precio de carrito completo para POS")
        .WithDescription("Determina el precio final de todos los artículos del carrito y aplica descuentos por volumen de pedido.")
        .Produces<CartResolutionResponse>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record ResolveCartRequest(
    Guid? CustomerId,
    SalesChannel Channel,
    string Currency,
    IReadOnlyList<ResolveCartItemRequest> Items);

public sealed record ResolveCartItemRequest(
    Guid ProductId,
    Guid? CategoryId,
    decimal Quantity);
