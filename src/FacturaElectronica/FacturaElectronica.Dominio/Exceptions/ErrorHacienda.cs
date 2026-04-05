using System.Text.Json.Serialization;

namespace FacturaElectronica.Dominio.Exceptions;

public class ErrorHacienda
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}