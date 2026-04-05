using FacturaElectronica.Aplicacion.Abstracciones;
using FacturaElectronica.Aplicacion.ProcesoNotaDebito.Enviar;
using MediatR;

namespace FacturaElectronica.WebApi.Endpoints.NotaDebito;

public class NotaDebitoElectronica : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("documentos/nota-debito", async (IMediator mediator, ITenantContext tenantContext, ProcesoNotaDebitoRequest notaDebito) =>
            {
                try
                {
                    notaDebito.TenantId = tenantContext.TenantId;
                    var command = new EnviarNotaDebitoCommand(notaDebito);
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
            .WithName("NotaDebitoElectronica")
            .WithDescription("Envía una Nota de Débito Electrónica a Hacienda. Requiere InformacionReferencia obligatoria.")
            .WithOpenApi();
    }
}
