using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Queries.GetCustomerGroups;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class GetCustomerGroupsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pricing/customer-groups", async (
            bool? isActive,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCustomerGroupsQuery(isActive);
            var result = await mediator.Send(query, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description);
        })
        .WithName("GetCustomerGroups")
        .WithTags("Pricing")
        .Produces<IReadOnlyList<CustomerGroupSummaryResponse>>(200)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
