using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Modelos.Storage;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Local;

public class LocalStorageService: IEstrategiaStorage
{
    public async Task<string> GuardarDocumentoFisicoAsync(StorageRequest request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.Content))
            throw new ArgumentException("El contenido del documento no puede estar vacío.");
        
        var date = DateTime.UtcNow;
        
        var basePath = $"/storage/facturas/{date:yyyy}/{date:MM}/{date:dd}/{request.BucketName ?? "default"}";
        var nombreArchivo = request.DocumentName ?? $"{Guid.NewGuid()}.xml";
        var rutaCompleta = Path.Combine(basePath, nombreArchivo);

        Directory.CreateDirectory(Path.GetDirectoryName(rutaCompleta)!);
        var content = System.Text.Encoding.UTF8.GetBytes(request.Content);
        await File.WriteAllBytesAsync(rutaCompleta, content, cancellationToken);

        return rutaCompleta; // Devuelve la ruta para guardar en DB
    }
}