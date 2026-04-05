namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;

/// <summary>
/// Modelo que representa un documento almacenado
/// </summary>
public class DocumentoAlmacenado
{
    public string Clave { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    
    // Contenido de los XMLs
    public string XmlSinFirmar { get; set; } = string.Empty;
    public string XmlFirmado { get; set; } = string.Empty;
    public string? XmlRespuesta { get; set; }
    
    // Rutas/URLs según el proveedor
    public string RutaSinFirmar { get; set; } = string.Empty;
    public string RutaFirmado { get; set; } = string.Empty;
    public string? RutaRespuesta { get; set; }
    
    // Metadata
    public long TamañoTotalBytes { get; set; }
    public string ProveedorStorage { get; set; } = string.Empty;
    public Dictionary<string, string> MetadataAdicional { get; set; } = new();
}