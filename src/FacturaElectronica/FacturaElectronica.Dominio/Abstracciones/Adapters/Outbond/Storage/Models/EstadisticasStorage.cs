namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;

/// <summary>
/// Estadísticas de uso del storage
/// </summary>
public class EstadisticasStorage
{
    public long TotalDocumentos { get; set; }
    public long TotalBytes { get; set; }
    public DateTime FechaPrimerDocumento { get; set; }
    public DateTime FechaUltimoDocumento { get; set; }
    public Dictionary<string, long> DocumentosPorTipo { get; set; } = new();
}