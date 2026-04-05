using System.Text.Json.Serialization;

namespace FacturaElectronica.Dominio.Modelos.Fiscal;

/// <summary>
/// Modelos de request/response para Hacienda
/// </summary>
public class RecepcionDocumentoRequest
{
    [JsonPropertyName("clave")]
    public string Clave { get; set; } = string.Empty;

    [JsonPropertyName("fecha")]
    public string Fecha { get; set; } = string.Empty;

    [JsonPropertyName("emisor")]
    public EmisorRequest Emisor { get; set; } = new();

    [JsonPropertyName("receptor")]
    public ReceptorRequest? Receptor { get; set; }

    [JsonPropertyName("comprobanteXml")]
    public string ComprobanteXml { get; set; } = string.Empty;

    [JsonPropertyName("callbackUrl")]
    public string? CallbackUrl { get; set; }
}

public class EmisorRequest
{
    [JsonPropertyName("tipoIdentificacion")]
    public string TipoIdentificacion { get; set; } = string.Empty;

    [JsonPropertyName("numeroIdentificacion")]
    public string NumeroIdentificacion { get; set; } = string.Empty;
}

public class ReceptorRequest
{
    [JsonPropertyName("tipoIdentificacion")]
    public string? TipoIdentificacion { get; set; }

    [JsonPropertyName("numeroIdentificacion")]
    public string? NumeroIdentificacion { get; set; }
}