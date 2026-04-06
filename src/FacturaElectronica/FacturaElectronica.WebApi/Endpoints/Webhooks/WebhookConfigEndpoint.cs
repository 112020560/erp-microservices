using FacturaElectronica.Aplicacion.Abstracciones;
using FacturaElectronica.Aplicacion.Webhooks;
using FacturaElectronica.Dominio.Entidades;
using MediatR;

namespace FacturaElectronica.WebApi.Endpoints.Webhooks;

public class WebhookConfigEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("webhooks").WithTags("Webhooks");

        group.MapGet("/", async (IMediator mediator, ITenantContext tenantContext) =>
        {
            var result = await mediator.Send(new GetNotificationConfigQuery(tenantContext.TenantId));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetNotificationConfig")
        .WithSummary("Obtener configuración de notificación del tenant")
        .WithOpenApi();

        group.MapPut("/", async (IMediator mediator, ITenantContext tenantContext, UpsertNotificationConfigRequest request) =>
        {
            var command = new UpsertNotificationConfigCommand(
                tenantContext.TenantId,
                request.Channel,
                request.WebhookUrl,
                request.WebhookSecret,
                request.IsActive,
                request.SubscribedEvents);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpsertNotificationConfig")
        .WithSummary("Crear o actualizar configuración de notificación")
        .WithOpenApi();

        group.MapDelete("/", async (IMediator mediator, ITenantContext tenantContext) =>
        {
            var deleted = await mediator.Send(new DeleteNotificationConfigCommand(tenantContext.TenantId));
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteNotificationConfig")
        .WithSummary("Eliminar configuración de notificación")
        .WithOpenApi();

        group.MapPost("/test", async (IMediator mediator, ITenantContext tenantContext) =>
        {
            var result = await mediator.Send(new TestWebhookCommand(tenantContext.TenantId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("TestWebhook")
        .WithSummary("Enviar webhook de prueba")
        .WithOpenApi();
    }
}

public sealed record UpsertNotificationConfigRequest(
    NotificationChannel Channel,
    string? WebhookUrl,
    string? WebhookSecret,
    bool IsActive,
    string SubscribedEvents);
