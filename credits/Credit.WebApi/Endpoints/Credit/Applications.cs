namespace Credit.WebApi.Endpoints.Credit;

public class Applications: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/credit/applications", )
    }
}