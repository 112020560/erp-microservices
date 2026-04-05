namespace FacturaElectronica.Dominio.Settings;

/// <summary>
/// Opciones de configuración para los proveedores
/// </summary>
public class StorageOptions
{
    public string ProveedorPorDefecto { get; set; } = "FileSystem";
    public string RutaBase { get; set; } = "./documentos-electronicos";
    public bool ComprimirDespuesDeMeses { get; set; } = true;
    public int MesesAntesDeComprimir { get; set; } = 6;
    public bool GuardarMetadata { get; set; } = true;
}