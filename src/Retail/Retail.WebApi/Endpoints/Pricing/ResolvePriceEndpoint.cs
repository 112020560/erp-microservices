using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.ResolvePrice;
using Retail.Domain.Pricing;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class ResolvePriceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/resolve", async (
            ResolvePriceRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var items = request.Items
                .Select(i => new PriceRequestItem(i.ProductId, i.CategoryId, i.Quantity))
                .ToList()
                .AsReadOnly();

            var query = new ResolvePriceQuery(request.CustomerId, request.Channel, items, request.Currency);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description);
        })
        .WithName("ResolvePrice")
        .WithTags("Pricing")
        .WithSummary("Resolver precio para el POS")
        .WithDescription("Determina el precio final para una lista de productos dado un contexto de venta (cliente, canal, cantidad).")
        .Produces<IReadOnlyList<ResolvedPriceResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record ResolvePriceRequest(
    Guid? CustomerId,
    SalesChannel Channel,
    string Currency,
    IReadOnlyList<ResolvePriceItemRequest> Items);

public sealed record ResolvePriceItemRequest(
    Guid ProductId,
    Guid CategoryId,
    decimal Quantity);
