using FacturaElectronica.Aplicacion.Abstracciones;
using FacturaElectronica.Aplicacion.ProcesoNotaCredito.Enviar;
using MediatR;

namespace FacturaElectronica.WebApi.Endpoints.NotaCredito;

public class NotaCreditoElectronica : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("documentos/nota-credito", async (IMediator mediator, ITenantContext tenantContext, ProcesoNotaCreditoRequest notaCredito) =>
            {
                try
                {
                    notaCredito.TenantId = tenantContext.TenantId;
                    var command = new EnviarNotaCreditoCommand(notaCredito);
                    var resultado = await mediator.Send(command);

                    return resultado.Exitoso
                        ? Results.Ok(resultado)
                        : Results.BadRequest(resultado);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            })
            .WithTags("Documentos")
            .WithName("NotaCreditoElectronica")
            .WithDescription("Envía una Nota de Crédito Electrónica a Hacienda. Requiere InformacionReferencia obligatoria.")
            .WithOpenApi();
    }
}
