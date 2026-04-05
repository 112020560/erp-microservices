using FacturaElectronica.Aplicacion.Abstracciones;

namespace FacturaElectronica.WebApi.Middleware;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
}
