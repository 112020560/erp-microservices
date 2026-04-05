namespace FacturaElectronica.Dominio.Settings;

public class HaciendaSettings
{
    /// <summary>
    /// URL base de la API de Hacienda
    /// Staging: https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1
    /// Producción: https://api.comprobanteselectronicos.go.cr/recepcion/v1
    /// </summary>
    public string ApiUrl { get; set; } = null!;
    
    /// <summary>
    /// URL del servicio de autenticación (ATV)
    /// Staging: https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token
    /// Producción: https://idp.comprobanteselectronicos.go.cr/auth/realms/rut/protocol/openid-connect/token
    /// </summary>
    public string ApiOauthUrl { get; set; } = null!;
    
    /// <summary>
    /// Client ID (api-stag para staging, api-prod para producción)
    /// </summary>
    public string ClientId { get; set; } = null!;
    
    /// <summary>
    /// Usuario (cédula del usuario autorizado)
    /// </summary>
    public string UserName { get; set; } = null!;
    
    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public string Password { get; set; } = null!;
    
    /// <summary>
    /// Ambiente (staging o production)
    /// </summary>
    public string Ambiente { get; set; } = "staging";
    
    /// <summary>
    /// Indica si está en producción
    /// </summary>
    public bool EsProduccion => Ambiente.Equals("production", StringComparison.OrdinalIgnoreCase);
    
    public int TimeoutSegundos { get; set; } = 30;
    
    public bool UsarAmbientePruebas { get; set; } = true;
    
    public string ApiUrlRecepcion { get; set; } = string.Empty;
    public string ApiUrlConsulta { get; set; } = string.Empty;
    public int MaxReintentos { get; set; } = 3;
}
