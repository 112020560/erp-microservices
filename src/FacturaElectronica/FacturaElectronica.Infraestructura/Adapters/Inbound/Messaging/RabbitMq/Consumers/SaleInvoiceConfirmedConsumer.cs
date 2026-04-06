using FacturaElectronica.Aplicacion.ProcesoFactura.Enviar;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Contracts.Sales;

namespace FacturaElectronica.Infraestructura.Adapters.Inbound.Messaging.RabbitMq.Consumers;

public sealed class SaleInvoiceConfirmedConsumer(
    ILogger<SaleInvoiceConfirmedConsumer> logger,
    ITenantRepository tenantRepository,
    IMediator mediator) : IConsumer<SaleInvoiceConfirmedEvent>
{
    public async Task Consume(ConsumeContext<SaleInvoiceConfirmedEvent> context)
    {
        var msg = context.Message;

        // Only process if electronic invoice is required and TenantId is present
        if (!msg.RequiresElectronicInvoice || !msg.TenantId.HasValue)
        {
            logger.LogInformation(
                "Invoice {InvoiceNumber} does not require electronic invoice or has no TenantId. Skipping.",
                msg.InvoiceNumber);
            return;
        }

        var tenantId = msg.TenantId.Value;

        logger.LogInformation(
            "Processing SaleInvoiceConfirmedEvent for invoice {InvoiceNumber}, TenantId: {TenantId}",
            msg.InvoiceNumber, tenantId);

        // Load tenant with emitter config
        var tenant = await tenantRepository.GetWithConfigsAsync(tenantId, context.CancellationToken);
        if (tenant is null || tenant.EmitterConfig is null)
        {
            logger.LogError(
                "Tenant {TenantId} not found or has no EmitterConfig. Cannot process invoice {InvoiceNumber}.",
                tenantId, msg.InvoiceNumber);
            return;
        }

        var emitter = tenant.EmitterConfig;
        var taxRate = 13m; // Costa Rica standard IVA

        // Build line details
        var detalles = msg.Lines.Select((line, index) =>
        {
            var subtotal = line.UnitPrice * line.Quantity;
            var descuento = subtotal - line.LineTotal; // discount = expected subtotal - actual lineTotal
            if (descuento < 0) descuento = 0;
            var montoImpuesto = Math.Round(line.LineTotal * taxRate / 100, 2);
            var montoTotalLinea = line.LineTotal + montoImpuesto;

            return new DetalleServicioRequest
            {
                NumeroLinea = index + 1,
                CodigoArticulo = line.Sku,
                UnidadMedida = "Unid",
                DetalleArticulo = line.ProductName,
                Cantidad = (int)line.Quantity,
                PrecioUnitario = line.UnitPrice,
                Precio = subtotal,
                Descuento = Math.Round(descuento, 2),
                NaturalezaDescuento = "Descuento al Cliente",
                SubTotal = line.LineTotal,
                CodigoImpuesto = "01",
                TarifaImpuesto = taxRate,
                MontoImpuesto = montoImpuesto,
                MontoTotalLinea = Math.Round(montoTotalLinea, 2)
            };
        }).ToList();

        // Calculate totals
        var totalVenta = msg.Lines.Sum(l => l.UnitPrice * l.Quantity);
        var totalDescuentos = totalVenta - msg.Lines.Sum(l => l.LineTotal);
        if (totalDescuentos < 0) totalDescuentos = 0;
        var totalVentaNeta = msg.Lines.Sum(l => l.LineTotal);
        var totalImpuesto = Math.Round(totalVentaNeta * taxRate / 100, 2);
        var totalComprobante = totalVentaNeta + totalImpuesto;

        // Use InvoiceId hash as consecutive number (deterministic, unique per invoice)
        var consecutivo = Math.Abs(msg.InvoiceId.GetHashCode() % 1_000_000L) + 1;

        var request = new ProcesoFacturaRequest
        {
            TenantId = tenantId,
            ExternalDocumentId = msg.InvoiceNumber,
            TipoDocumento = "01",
            ConsecutivoDocumento = consecutivo,
            FechaDocumento = msg.ConfirmedAt.UtcDateTime,
            CondicionVenta = "01",   // contado
            MedioPago = "01",        // efectivo (default)
            CodigoMoneda = msg.Currency,
            TipoCambio = "1.0000",

            // Emisor from TenantEmitterConfig
            EmisorNombre = emitter.Nombre,
            EmisorTipoIdentificacion = emitter.TipoIdentificacion,
            EmisorNumeroIdentificacion = emitter.NumeroIdentificacion,
            EmisorProvincia = emitter.Provincia,
            EmisorCanton = emitter.Canton,
            EmisorDistrito = emitter.Distrito,
            EmisorBarrio = emitter.Barrio,
            EmisorOtrasSenas = emitter.OtrasSenas,
            EmisorCorreoElectronico = emitter.CorreoElectronico,
            EmisorTelefono = emitter.Telefono,

            // Receptor — walk-in customers without fiscal ID
            Receptor = false,
            ReceptorNombre = msg.CustomerName,
            ReceptorTipoIdentificacion = null,
            ReceptorNumeroIdentificacion = null,
            ReceptorCodigoPais = "506",

            // Line details
            DetalleServicios = detalles,

            // Totals
            TotalServGravados = 0,
            TotalServExentos = 0,
            TotalMercanciasGravadas = totalVentaNeta,
            TotalMercanciasExentas = 0,
            TotalGravado = totalVentaNeta,
            TotalExento = 0,
            TotalVenta = Math.Round(totalVenta, 2),
            TotalDescuentos = Math.Round(totalDescuentos, 2),
            TotalVentaNeta = Math.Round(totalVentaNeta, 2),
            TotalImpuesto = totalImpuesto,
            TotalComprobante = Math.Round(totalComprobante, 2)
        };

        var command = new EnviarFacturaCommand(request);
        await mediator.Send(command, context.CancellationToken);

        logger.LogInformation(
            "Electronic invoice command sent for invoice {InvoiceNumber}, TenantId: {TenantId}",
            msg.InvoiceNumber, tenantId);
    }
}
