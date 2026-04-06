using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public record DeleteNotificationConfigCommand(Guid TenantId) : IRequest<bool>;
