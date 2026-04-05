namespace FacturaElectronica.Aplicacion.Abstracciones;

public interface ITenantContext
{
    Guid TenantId { get; }
}
