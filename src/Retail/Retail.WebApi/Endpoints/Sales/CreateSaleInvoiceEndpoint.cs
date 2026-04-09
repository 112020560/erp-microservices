using Asp.Versioning;
using MediatR;
using Retail.Application.Sales.Commands.CreateSaleInvoice;
using Retail.Domain.Sales;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class CreateSaleInvoiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/sales/invoices", async (
            CreateSaleInvoiceRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSaleInvoiceCommand(
                request.QuoteId,
                request.CashierId,
                request.RequiresElectronicInvoice,
                request.TenantId,
                request.Payments.Select(p => new CreatePaymentLineDto(p.Method, p.Amount, p.Reference)).ToList(),
                request.CreditProductId);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/sales/invoices/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("CreateSaleInvoice")
        .WithTags("Sales")
        .WithSummary("Facturar cotización")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record CreateSaleInvoiceRequest(
    Guid QuoteId,
    Guid CashierId,
    bool RequiresElectronicInvoice,
    Guid? TenantId,
    IReadOnlyList<CreatePaymentLineRequest> Payments,
    Guid? CreditProductId = null);

public sealed record CreatePaymentLineRequest(PaymentMethod Method, decimal Amount, string? Reference);
