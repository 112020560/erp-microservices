using FacturaElectronica.Aplicacion.Abstracciones;
using FacturaElectronica.Aplicacion.ProcesoFactura;
using FacturaElectronica.Aplicacion.ProcesoFactura.Consultar;
using FacturaElectronica.Aplicacion.ProcesoFactura.Enviar;
using MediatR;

namespace FacturaElectronica.WebApi.Endpoints.Factura;

public class FacturaElectronica : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("documentos/factura-electronica", async (IMediator mediator, ITenantContext tenantContext, ProcesoFacturaRequest factura) =>
            {
                try
                {
                    factura.TenantId = tenantContext.TenantId;
                    var command = new EnviarFacturaCommand(factura);
                    var resultado = await mediator.Send(command);
                    return Results.Ok(resultado);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            })
            .WithTags("Documentos")
            .WithName("FacturaElectronica")
            .WithOpenApi();

        app.MapGet("documentos/factura-electronica/{clave}", ConsultaFacturaElectronicaHandler)
            .WithTags("Documentos")
            .WithName("ConsultaFacturaElectronica")
            .WithOpenApi();
    }

    private async Task<IResult> ConsultaFacturaElectronicaHandler(IMediator mediator, ITenantContext tenantContext, string clave)
    {
        try
        {
            var query = new ConsultarDocumentoQuery(clave, tenantContext.TenantId);
            var resultado = await mediator.Send(query);
            return Results.Ok(resultado);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
