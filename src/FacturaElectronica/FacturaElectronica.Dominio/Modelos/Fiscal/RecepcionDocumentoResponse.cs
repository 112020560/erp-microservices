using System.Text.Json.Serialization;

namespace FacturaElectronica.Dominio.Modelos.Fiscal;

public class RecepcionDocumentoResponse
{
    [JsonPropertyName("clave")]
    public string? Clave { get; set; }

    [JsonPropertyName("fecha")]
    public string? Fecha { get; set; }

    [JsonPropertyName("indEstado")]
    public string? IndicadorEstado { get; set; }

    [JsonPropertyName("respuestaXml")]
    public string? RespuestaXml { get; set; }
}