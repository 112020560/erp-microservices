using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// IMPLEMENTACIÓN DEL FACTORY (INFRAESTRUCTURA)
/// ═══════════════════════════════════════════════════════════════
/// Esta clase SÍ conoce las implementaciones concretas
/// porque vive en Infraestructura
/// </summary>
public class StorageProviderFactory : IStorageProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StorageOptions _options;
    private readonly ILogger<StorageProviderFactory> _logger;
    
    // Registro dinámico de proveedores
    private readonly Dictionary<string, Type> _proveedoresRegistrados;

    public StorageProviderFactory(
        IServiceProvider serviceProvider,
        IOptions<StorageOptions> options,
        ILogger<StorageProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
        _proveedoresRegistrados = new Dictionary<string, Type>();
    }

    public IStorageProvider CrearProveedor(string nombreProveedor)
    {
        _logger.LogInformation("Creando proveedor de storage: {Proveedor}", nombreProveedor);

        if (!_proveedoresRegistrados.ContainsKey(nombreProveedor))
        {
            throw new NotSupportedException(
                $"Proveedor '{nombreProveedor}' no está registrado. " +
                $"Proveedores disponibles: {string.Join(", ", _proveedoresRegistrados.Keys)}");
        }

        var tipoProveedor = _proveedoresRegistrados[nombreProveedor];
        
        try
        {
            var proveedor = (IStorageProvider)_serviceProvider.GetRequiredService(tipoProveedor);
            _logger.LogDebug("Proveedor {Proveedor} creado exitosamente", nombreProveedor);
            return proveedor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando proveedor {Proveedor}", nombreProveedor);
            throw new InvalidOperationException(
                $"No se pudo crear el proveedor '{nombreProveedor}'. " +
                $"Asegúrate de que esté registrado en DI.", ex);
        }
    }

    public IStorageProvider CrearProveedorPorDefecto()
    {
        var proveedorDefault = _options.ProveedorPorDefecto ?? "FileSystem";
        _logger.LogInformation("Creando proveedor por defecto: {Proveedor}", proveedorDefault);
        return CrearProveedor(proveedorDefault);
    }

    public IEnumerable<string> ObtenerProveedoresDisponibles()
    {
        return _proveedoresRegistrados.Keys;
    }

    public void RegistrarProveedor(string nombre, Type tipoProveedor)
    {
        if (!typeof(IStorageProvider).IsAssignableFrom(tipoProveedor))
        {
            throw new ArgumentException(
                $"El tipo {tipoProveedor.Name} debe implementar IStorageProvider",
                nameof(tipoProveedor));
        }

        _logger.LogInformation(
            "Registrando proveedor: {Nombre} -> {Tipo}",
            nombre, tipoProveedor.Name);

        _proveedoresRegistrados[nombre] = tipoProveedor;
    }

    /// <summary>
    /// Método auxiliar para validar si un proveedor está disponible
    /// </summary>
    public bool EstaDisponible(string nombreProveedor)
    {
        return _proveedoresRegistrados.ContainsKey(nombreProveedor);
    }
}