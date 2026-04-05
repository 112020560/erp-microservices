using FacturaElectronica.Dominio.Modelos.Storage;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;

public interface IEstrategiaStorage
{
    Task<string> GuardarDocumentoFisicoAsync(StorageRequest request, CancellationToken cancellationToken);
}