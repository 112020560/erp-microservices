using FacturaElectronica.Aplicacion.ProcesoFactura.Polling;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FacturaElectronica.Infraestructura.Adapters.Inbound.Jobs;

public class PoolingInvoiceJob: IJob
{
    private readonly ILogger<PoolingInvoiceJob> _logger;
    private readonly IPollingFacturasService _pollingFacturasService;
    public PoolingInvoiceJob(ILogger<PoolingInvoiceJob> logger, IPollingFacturasService pollingFacturasService)
    {
        _logger = logger;
        _pollingFacturasService = pollingFacturasService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Iniciando el job de polling de facturas electrónicas.");
        try
        {
            await _pollingFacturasService.ExecuteAsync(context.CancellationToken);
            _logger.LogInformation("Job de polling de facturas electrónicas finalizado correctamente.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error ejecutando el job de polling de facturas electrónicas.");
        }
    }
}