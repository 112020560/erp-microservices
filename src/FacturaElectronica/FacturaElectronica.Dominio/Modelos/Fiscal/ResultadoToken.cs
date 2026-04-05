using System.Text.Json.Serialization;

namespace FacturaElectronica.Dominio.Modelos.Fiscal;

public class ResultadoToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";
    
    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; }
    
    [JsonPropertyName("session_state")]
    public string? SessionState { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    /// <summary>
    /// Fecha en que se obtuvo el token (para calcular expiración)
    /// </summary>
    [JsonIgnore]
    public DateTime FechaObtencion { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica si el token está vigente
    /// </summary>
    [JsonIgnore]
    public bool EstaVigente
    {
        get
        {
            // Considerar expirado 1 minuto antes para evitar race conditions
            var tiempoRestante = FechaObtencion.AddSeconds(ExpiresIn) - DateTime.UtcNow;
            return tiempoRestante.TotalSeconds > 60;
        }
    }
}