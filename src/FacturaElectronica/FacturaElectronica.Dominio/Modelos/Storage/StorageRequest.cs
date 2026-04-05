namespace FacturaElectronica.Dominio.Modelos.Storage;

public class StorageRequest
{
    public string? TenantId { get; set; }
    public string? Content { get; set; }
    public string? DocumentName { get; set; }
    public string? BucketName { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}

public enum EstadoFacturaStorage
{
    Pendiente,
    Almacenado,
    Error
}