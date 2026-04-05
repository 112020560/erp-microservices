using FacturaElectronica.Aplicacion.Abstracciones;
using FacturaElectronica.Aplicacion.ProcesoFactura.DescargarDocumento;
using FacturaElectronica.Aplicacion.ProcesoFactura.Listar;
using FacturaElectronica.Aplicacion.ProcesoFactura.MarcarCorreccion;
using FacturaElectronica.Aplicacion.ProcesoFactura.ObtenerDetalle;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace FacturaElectronica.WebApi.Endpoints.Factura;

public class GestionFacturasElectronicas : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("facturas-electronicas")
            .WithTags("Gestión Facturas Electrónicas")
            .WithOpenApi();

        group.MapGet("", ListarFacturasHandler)
            .WithName("ListarFacturasElectronicas")
            .WithSummary("Lista facturas electrónicas con filtros y paginación");

        group.MapGet("{id:guid}", ObtenerDetalleHandler)
            .WithName("ObtenerDetalleFacturaElectronica")
            .WithSummary("Obtiene el detalle de una factura electrónica incluyendo logs");

        group.MapPost("{id:guid}/marcar-para-correccion", MarcarParaCorreccionHandler)
            .WithName("MarcarFacturaParaCorreccion")
            .WithSummary("Marca una factura para corrección (solo rechazadas o con error)");

        var groupPublic = app.MapGroup("public/facturas-electronicas")
            .WithTags("Gestión Facturas Electrónicas - Público")
            .WithOpenApi();

        groupPublic.MapGet("{id:guid}/documentos/{tipo}", DescargarDocumentoHandler)
            .WithName("DescargarDocumentoFacturaElectronica")
            .WithSummary("Descarga un documento XML (sin-firmar, firmado, respuesta)");
    }

    private async Task<IResult> ListarFacturasHandler(
        IMediator mediator,
        ITenantContext tenantContext,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] string? emisorId = null,
        [FromQuery] string? receptorId = null,
        [FromQuery] bool? requiereCorreccion = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new ListarFacturasQuery(
            tenantContext.TenantId,
            status,
            fechaDesde,
            fechaHasta,
            emisorId,
            receptorId,
            requiereCorreccion,
            page,
            pageSize);

        var resultado = await mediator.Send(query);
        return Results.Ok(ApiResponse.SuccessWithData(resultado, 0));
    }

    private async Task<IResult> ObtenerDetalleHandler(
        IMediator mediator,
        ITenantContext tenantContext,
        Guid id)
    {
        var query = new ObtenerDetalleFacturaQuery(id, tenantContext.TenantId);
        var resultado = await mediator.Send(query);

        if (resultado == null)
            return Results.NotFound(new { message = "Factura electrónica no encontrada" });

        return Results.Ok(ApiResponse.SuccessWithData(resultado, 0));
    }

    private async Task<IResult> DescargarDocumentoHandler(
        IMediator mediator,
        ITenantContext tenantContext,
        Guid id,
        string tipo)
    {
        var tiposValidos = new[] { "sin-firmar", "firmado", "respuesta" };
        if (!tiposValidos.Contains(tipo.ToLower()))
        {
            return Results.BadRequest(new { message = "Tipo de documento inválido. Valores válidos: sin-firmar, firmado, respuesta" });
        }

        var query = new DescargarDocumentoQuery(id, tenantContext.TenantId, tipo);
        var resultado = await mediator.Send(query);

        if (resultado == null)
            return Results.NotFound(new { message = "Documento no encontrado" });

        return Results.File(
            resultado.Content,
            resultado.ContentType,
            resultado.FileName);
    }

    private async Task<IResult> MarcarParaCorreccionHandler(
        IMediator mediator,
        ITenantContext tenantContext,
        Guid id,
        [FromBody] MarcarParaCorreccionRequest request)
    {
        var command = new MarcarParaCorreccionCommand(id, tenantContext.TenantId, request.Notas);
        var resultado = await mediator.Send(command);

        if (!resultado.Success)
            return Results.BadRequest(new { message = resultado.Message });

        return Results.Ok(ApiResponse.SuccessWithData(new { message = resultado.Message }, 0));
    }
}

public class MarcarParaCorreccionRequest
{
    public string? Notas { get; set; }
}
