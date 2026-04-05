namespace FacturaElectronica.Dominio.Settings;

public class ConfiguracionFacturaElectronica
{
    public bool CertificadoLocal { get; set; }
    public string? NombreCertificadoDigital { get; set; }
    public string? ClaveCertificadoDigital { get; set; }
    //OAuth2
    public string ClientId { get; set; } = "api-stag";
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public int MaximoReintentos { get; set; }
    public string? CallbackUrl { get; set; }
    public string EstrategiaAlmacenamiento { get; set; } = "local";
}