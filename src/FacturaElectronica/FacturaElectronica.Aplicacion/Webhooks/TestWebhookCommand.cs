using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public record TestWebhookCommand(Guid TenantId) : IRequest<TestWebhookResponse>;

public record TestWebhookResponse(bool Success, string Message);
