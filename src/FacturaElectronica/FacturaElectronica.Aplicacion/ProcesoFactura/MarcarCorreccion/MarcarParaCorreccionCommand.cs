using MediatR;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.MarcarCorreccion;

public record MarcarParaCorreccionCommand(Guid Id, Guid TenantId, string? Notas) : IRequest<MarcarParaCorreccionResponse>;

public class MarcarParaCorreccionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
