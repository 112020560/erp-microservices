namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// FACTORY PARA CREAR PROVEEDORES (SOLO INTERFAZ EN DOMINIO)
/// ═══════════════════════════════════════════════════════════════
/// La interfaz vive en el Dominio
/// La implementación vive en Infraestructura
/// </summary>
public interface IStorageProviderFactory
{
    IStorageProvider CrearProveedor(string nombreProveedor);
    IStorageProvider CrearProveedorPorDefecto();
    IEnumerable<string> ObtenerProveedoresDisponibles();
    void RegistrarProveedor(string nombre, Type tipoProveedor);
}