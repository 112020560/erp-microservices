namespace FacturaElectronica.Dominio.Entidades;

public class TenantEmitterConfig
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Canton { get; set; } = string.Empty;
    public string Distrito { get; set; } = string.Empty;
    public string Barrio { get; set; } = string.Empty;
    public string OtrasSenas { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
