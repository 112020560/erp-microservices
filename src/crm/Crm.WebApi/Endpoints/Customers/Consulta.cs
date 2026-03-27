using System;
using Asp.Versioning;
using Crm.Application.Customers;
using Crm.WebApi.Extensions;
using MediatR;

namespace Crm.WebApi.Endpoints.Customers;

public class Consulta : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/customers/{customerId}", async (Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerByIdQuery(customerId));
            return result.Match(
                customer => Results.Ok(customer),
                _ => Results.NotFound());
        })
        .WithName("GetCustomerById")
        .WithTags("Customers")
        .Produces(200)
        .Produces(404)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
