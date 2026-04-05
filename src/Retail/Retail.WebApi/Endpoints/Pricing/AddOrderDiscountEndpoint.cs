using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AddOrderDiscount;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AddOrderDiscountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/price-lists/{id:guid}/order-discounts", async (
            Guid id,
            AddOrderDiscountRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AddOrderDiscountCommand(
                id,
                request.MinOrderTotal,
                request.MinOrderQuantity,
                request.DiscountPercentage,
                request.DiscountAmount,
                request.MaxDiscountAmount,
                request.Priority);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/price-lists/{id}/order-discounts/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AddOrderDiscount")
        .WithTags("Pricing")
        .WithSummary("Agregar descuento por volumen de pedido a una lista de precios")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AddOrderDiscountRequest(
    decimal? MinOrderTotal,
    decimal? MinOrderQuantity,
    decimal DiscountPercentage,
    decimal? DiscountAmount,
    decimal? MaxDiscountAmount,
    int Priority);
