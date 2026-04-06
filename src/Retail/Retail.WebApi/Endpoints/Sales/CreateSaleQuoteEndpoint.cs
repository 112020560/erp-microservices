using Asp.Versioning;
using MediatR;
using Retail.Application.Sales.Commands.CreateSaleQuote;
using Retail.Domain.Pricing;
using Retail.Domain.Sales;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class CreateSaleQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/sales/quotes", async (
            CreateSaleQuoteRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSaleQuoteCommand(
                request.SalesPersonId,
                request.CustomerId,
                request.CustomerName,
                request.WarehouseId,
                request.Channel,
                request.ValidUntil,
                request.Currency,
                request.Notes,
                request.Subtotal,
                request.VolumeDiscountAmount,
                request.PromotionDiscountAmount,
                request.TaxAmount,
                request.Total,
                request.Lines.Select(l => new CreateSaleQuoteLineDto(
                    l.ProductId, l.Sku, l.ProductName, l.CategoryId,
                    l.Quantity, l.UnitPrice, l.DiscountPercentage, l.LineTotal,
                    l.PriceListName, l.ResolutionSource)).ToList(),
                request.AppliedPromotions.Select(p => new CreateAppliedPromotionDto(
                    p.PromotionId, p.PromotionName, p.DiscountAmount)).ToList());

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/sales/quotes/{result.Value}", new { Id = result.Value, QuoteNumber = $"COT-{DateTime.UtcNow.Year}" })
                : Results.Problem(result.Error.Description);
        })
        .WithName("CreateSaleQuote")
        .WithTags("Sales")
        .WithSummary("Crear cotización de venta")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record CreateSaleQuoteRequest(
    Guid SalesPersonId,
    Guid? CustomerId,
    string CustomerName,
    Guid WarehouseId,
    SalesChannel Channel,
    DateTimeOffset ValidUntil,
    string Currency,
    string? Notes,
    decimal Subtotal,
    decimal VolumeDiscountAmount,
    decimal PromotionDiscountAmount,
    decimal TaxAmount,
    decimal Total,
    IReadOnlyList<CreateSaleQuoteLineRequest> Lines,
    IReadOnlyList<CreateAppliedPromotionRequest> AppliedPromotions);

public sealed record CreateSaleQuoteLineRequest(
    Guid ProductId, string Sku, string ProductName, Guid? CategoryId,
    decimal Quantity, decimal UnitPrice, decimal DiscountPercentage, decimal LineTotal,
    string? PriceListName, string? ResolutionSource);

public sealed record CreateAppliedPromotionRequest(Guid PromotionId, string PromotionName, decimal DiscountAmount);
