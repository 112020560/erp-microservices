using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.GetCustomerGroupById;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetCustomerGroupByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/customer-groups/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCustomerGroupByIdQuery(id);
            var result = await mediator.Send(query, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description);
        })
        .WithName("GetCustomerGroupById")
        .WithTags("Pricing")
        .Produces<CustomerGroupDetailResponse>(200)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
