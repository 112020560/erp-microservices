using Asp.Versioning;
using MediatR;
using Retail.Application.Sales.Queries.GetSaleInvoiceById;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class GetSaleInvoiceByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales/invoices/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetSaleInvoiceByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        })
        .WithName("GetSaleInvoiceById")
        .WithTags("Sales")
        .WithSummary("Obtener factura de venta por ID")
        .Produces<SaleInvoiceDetailResponse>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
