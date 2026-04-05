namespace FacturaElectronica.Dominio.Entidades;

public class ElectronicInvoice
{
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant (empresa) propietaria de este documento electrónico
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// ID del documento en el sistema externo (ERP, POS, etc.) que originó la solicitud.
    /// Nullable: puede ser null si se crea directamente vía API REST.
    /// </summary>
    public string? ExternalDocumentId { get; set; }

    public string InvoiceType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? StatusDetail { get; set; }

    public string? Clave { get; set; }

    public string? Consecutivo { get; set; }

    public string? EmisorIdentificacion { get; set; }

    public string? ReceptorIdentificacion { get; set; }

    public string? XmlEmisorPath { get; set; }

    public string? XmlReceptorPath { get; set; }

    /// <summary>
    /// Ruta al archivo XML de respuesta de Hacienda
    /// </summary>
    public string? XmlRespuestaPath { get; set; }

    public DateTime FechaEmision { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public string? ResponseMessage { get; set; }

    public string? Error { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ProcessType { get; set; }

    /// <summary>
    /// Indica si la factura requiere corrección por parte del usuario
    /// </summary>
    public bool RequiereCorreccion { get; set; }

    /// <summary>
    /// Notas sobre la corrección requerida
    /// </summary>
    public string? NotasCorreccion { get; set; }

    /// <summary>
    /// Fecha en que se marcó para corrección
    /// </summary>
    public DateTime? FechaMarcadoCorreccion { get; set; }

    /// <summary>
    /// Correo electrónico del receptor (snapshot al momento de la factura)
    /// </summary>
    public string? CorreoReceptor { get; set; }

    /// <summary>
    /// Teléfono del receptor para SMS (snapshot al momento de la factura)
    /// </summary>
    public string? TelefonoReceptor { get; set; }

    /// <summary>
    /// Nombre del receptor para el email
    /// </summary>
    public string? NombreReceptor { get; set; }

    /// <summary>
    /// Indica si ya se envió la notificación al receptor
    /// </summary>
    public bool NotificacionEnviada { get; set; }

    /// <summary>
    /// Fecha en que se envió la notificación
    /// </summary>
    public DateTime? FechaNotificacion { get; set; }

    /// <summary>
    /// Logs de auditoría del documento
    /// </summary>
    public ICollection<ElectronicDocumentLog> Logs { get; set; } = [];
}
