namespace FacturaElectronica.Aplicacion.ProcesoFactura.Polling;

public interface IPollingFacturasService
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}