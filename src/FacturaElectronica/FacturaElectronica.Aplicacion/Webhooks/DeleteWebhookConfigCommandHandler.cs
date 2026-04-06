using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public class DeleteNotificationConfigCommandHandler(
    ITenantNotificationConfigRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteNotificationConfigCommand, bool>
{
    public async Task<bool> Handle(DeleteNotificationConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (config is null) return false;

        repository.Delete(config);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
