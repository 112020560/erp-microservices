using System.Text.Json.Serialization;

namespace FacturaElectronica.Dominio.Modelos.Fiscal;

public class ConsultaDocumentoResponse
{
    [JsonPropertyName("clave")]
    public string? Clave { get; set; }

    [JsonPropertyName("fecha")]
    public string? Fecha { get; set; }

    [JsonPropertyName("ind-estado")]
    public string? IndicadorEstado { get; set; }

    [JsonPropertyName("respuesta-xml")]
    public string? RespuestaXml { get; set; }
}