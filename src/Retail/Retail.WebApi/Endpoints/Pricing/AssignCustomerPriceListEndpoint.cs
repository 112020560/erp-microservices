using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AssignCustomerPriceList;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AssignCustomerPriceListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/customer-assignments", async (
            AssignCustomerRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AssignCustomerPriceListCommand(
                request.CustomerId, request.PriceListId, request.ValidFrom, request.ValidTo);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/customer-assignments/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AssignCustomerPriceList")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AssignCustomerRequest(
    Guid CustomerId,
    Guid PriceListId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo);
