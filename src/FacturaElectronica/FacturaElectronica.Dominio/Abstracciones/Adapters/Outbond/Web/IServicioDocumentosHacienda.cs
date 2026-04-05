using FacturaElectronica.Dominio.Modelos.Fiscal;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;

/// <summary>
/// Interfaz del servicio de documentos
/// </summary>
public interface IServicioDocumentosHacienda
{
    Task<RecepcionDocumentoResponse> RecepcionDocumentoAsync(
        string token,
        RecepcionDocumentoRequest request,
        CancellationToken cancellationToken = default);

    Task<ConsultaDocumentoResponse> ConsultarDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);
}