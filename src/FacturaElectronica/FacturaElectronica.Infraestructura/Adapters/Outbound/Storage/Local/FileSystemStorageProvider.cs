using System.Text;
using System.Text.Json;
using System.Xml;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Local;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// PROVEEDOR: SISTEMA DE ARCHIVOS LOCAL (Por defecto)
/// ═══════════════════════════════════════════════════════════════
/// Estructura de carpetas:
/// documentos-electronicos/
/// ├── activos/
/// │   └── 2025/
/// │       └── 01/
/// │           └── 29/
/// │               └── 50629012.../
/// │                   ├── 01-sin-firmar.xml
/// │                   ├── 02-firmado.xml
/// │                   ├── 03-respuesta.xml
/// │                   └── metadata.json
/// └── archivados/
///     └── 2024/
///         └── 07.tar.gz
/// </summary>
public class FileSystemStorageProvider : StorageProviderBase
{
    private readonly string _rutaActivos;
    private readonly string _rutaArchivados;

    public override string ProviderName => "FileSystem";

    public FileSystemStorageProvider(
        ILogger<FileSystemStorageProvider> logger,
        IOptions<StorageOptions> options)
        : base(logger, options.Value)
    {
        _rutaActivos = Path.Combine(Options.RutaBase, "activos");
        _rutaArchivados = Path.Combine(Options.RutaBase, "archivados");
        
        CrearEstructuraInicial();
    }

    private void CrearEstructuraInicial()
    {
        Directory.CreateDirectory(_rutaActivos);
        Directory.CreateDirectory(_rutaArchivados);
        Logger.LogInformation("Estructura de carpetas creada en: {Ruta}", Options.RutaBase);
    }

    // ═══════════════════════════════════════════════════════════════
    // MÉTODO ANTERIOR (mantenido por compatibilidad)
    // ═══════════════════════════════════════════════════════════════
    // ADVERTENCIA: Este método puede causar problemas de firma inválida
    // porque usa XmlDocument.Save() que puede re-formatear el XML.
    // Usar GuardarDocumentoCompletoConBytesAsync en su lugar.
    // ═══════════════════════════════════════════════════════════════
    public override async Task<DocumentoAlmacenado> GuardarDocumentoCompletoAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        XmlDocument xmlFirmado,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Guardando documento {Clave} en FileSystem", clave);

            // Crear estructura de carpetas
            var carpetaDocumento = CrearCarpetaDocumento(clave);

            // 1. Guardar XML sin firmar
            var rutaSinFirmar = Path.Combine(carpetaDocumento, "01-sin-firmar.xml");
            await GuardarXmlAsync(xmlSinFirmar, rutaSinFirmar, cancellationToken);

            // 2. Guardar XML firmado
            // ⚠️ PROBLEMA: xml.Save() puede re-formatear el XML
            var rutaFirmado = Path.Combine(carpetaDocumento, "02-firmado.xml");
            await GuardarXmlAsync(xmlFirmado, rutaFirmado, cancellationToken);

            // 3. Guardar respuesta (si existe)
            string? rutaRespuesta = null;
            if (!string.IsNullOrEmpty(xmlRespuesta))
            {
                rutaRespuesta = Path.Combine(carpetaDocumento, "03-respuesta.xml");
                await File.WriteAllTextAsync(rutaRespuesta, xmlRespuesta, Encoding.UTF8, cancellationToken);
            }

            // 4. Guardar metadata
            var metadata = GenerarMetadata(clave, xmlFirmado);
            var rutaMetadata = Path.Combine(carpetaDocumento, "metadata.json");
            await GuardarMetadataAsync(rutaMetadata, metadata, cancellationToken);

            // Calcular tamaños
            var tamañoTotal = new FileInfo(rutaSinFirmar).Length +
                            new FileInfo(rutaFirmado).Length +
                            (rutaRespuesta != null ? new FileInfo(rutaRespuesta).Length : 0);

            Logger.LogInformation(
                "Documento {Clave} guardado exitosamente. Tamaño: {Tamaño} bytes",
                clave, tamañoTotal);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = XmlToString(xmlFirmado),
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = rutaSinFirmar,
                RutaFirmado = rutaFirmado,
                RutaRespuesta = rutaRespuesta,
                TamañoTotalBytes = tamañoTotal,
                ProveedorStorage = ProviderName,
                MetadataAdicional = metadata
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave}", clave);
            throw;
        }
    }

    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Guarda preservando los bytes exactos del XML firmado
    /// ═══════════════════════════════════════════════════════════════
    /// Este método escribe los bytes exactos del XML firmado sin re-serializar,
    /// evitando el problema de "XML modificado después de firmado".
    /// </summary>
    public override async Task<DocumentoAlmacenado> GuardarDocumentoCompletoConBytesAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        byte[] bytesXmlFirmado,
        XmlDocument xmlFirmadoParaMetadata,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Guardando documento {Clave} en FileSystem (con bytes preservados)", clave);

            // Crear estructura de carpetas
            var carpetaDocumento = CrearCarpetaDocumento(clave);

            // 1. Guardar XML sin firmar (este no tiene firma, así que está OK)
            var rutaSinFirmar = Path.Combine(carpetaDocumento, "01-sin-firmar.xml");
            await GuardarXmlAsync(xmlSinFirmar, rutaSinFirmar, cancellationToken);

            // 2. ═══════════════════════════════════════════════════════════════
            //    CRÍTICO: Guardar XML firmado usando los BYTES EXACTOS
            //    NO usar XmlDocument.Save() que puede re-formatear
            // ═══════════════════════════════════════════════════════════════
            var rutaFirmado = Path.Combine(carpetaDocumento, "02-firmado.xml");
            await File.WriteAllBytesAsync(rutaFirmado, bytesXmlFirmado, cancellationToken);

            // 3. Guardar respuesta (si existe)
            string? rutaRespuesta = null;
            if (!string.IsNullOrEmpty(xmlRespuesta))
            {
                rutaRespuesta = Path.Combine(carpetaDocumento, "03-respuesta.xml");
                await File.WriteAllTextAsync(rutaRespuesta, xmlRespuesta, Encoding.UTF8, cancellationToken);
            }

            // 4. Guardar metadata (usando XmlDocument solo para lectura)
            var metadata = GenerarMetadata(clave, xmlFirmadoParaMetadata);
            var rutaMetadata = Path.Combine(carpetaDocumento, "metadata.json");
            await GuardarMetadataAsync(rutaMetadata, metadata, cancellationToken);

            // Calcular tamaños
            var tamañoTotal = new FileInfo(rutaSinFirmar).Length +
                            new FileInfo(rutaFirmado).Length +
                            (rutaRespuesta != null ? new FileInfo(rutaRespuesta).Length : 0);

            Logger.LogInformation(
                "Documento {Clave} guardado exitosamente (bytes preservados). Tamaño: {Tamaño} bytes",
                clave, tamañoTotal);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = Encoding.UTF8.GetString(bytesXmlFirmado), // Usar los bytes originales
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = rutaSinFirmar,
                RutaFirmado = rutaFirmado,
                RutaRespuesta = rutaRespuesta,
                TamañoTotalBytes = tamañoTotal,
                ProveedorStorage = ProviderName,
                MetadataAdicional = metadata
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave}", clave);
            throw;
        }
    }

    public override async Task ActualizarRespuestaAsync(
        string clave,
        string xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var carpetaDocumento = ObtenerCarpetaDocumento(clave);
            
            if (!Directory.Exists(carpetaDocumento))
            {
                throw new FileNotFoundException($"No se encontró el documento {clave}");
            }

            var rutaRespuesta = Path.Combine(carpetaDocumento, "03-respuesta.xml");
            
            var xmlString = DecodeBase64ToXml(xmlRespuesta);
            await File.WriteAllTextAsync(rutaRespuesta, xmlString, Encoding.UTF8, cancellationToken);

            // Actualizar metadata
            var rutaMetadata = Path.Combine(carpetaDocumento, "metadata.json");
            if (File.Exists(rutaMetadata))
            {
                var metadataJson = await File.ReadAllTextAsync(rutaMetadata, cancellationToken);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson) 
                    ?? new Dictionary<string, string>();
                
                metadata["fecha_respuesta"] = DateTime.UtcNow.ToString("O");
                metadata["tiene_respuesta"] = "true";

                await GuardarMetadataAsync(rutaMetadata, metadata, cancellationToken);
            }

            Logger.LogInformation("Respuesta actualizada para documento {Clave}", clave);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error actualizando respuesta para {Clave}", clave);
            throw;
        }
    }

    public override async Task<DocumentoAlmacenado?> ObtenerDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var carpetaDocumento = ObtenerCarpetaDocumento(clave);
            
            if (!Directory.Exists(carpetaDocumento))
            {
                Logger.LogWarning("Documento {Clave} no encontrado", clave);
                return null;
            }

            var rutaSinFirmar = Path.Combine(carpetaDocumento, "01-sin-firmar.xml");
            var rutaFirmado = Path.Combine(carpetaDocumento, "02-firmado.xml");
            var rutaRespuesta = Path.Combine(carpetaDocumento, "03-respuesta.xml");
            var rutaMetadata = Path.Combine(carpetaDocumento, "metadata.json");

            var xmlSinFirmar = await File.ReadAllTextAsync(rutaSinFirmar, cancellationToken);
            var xmlFirmado = await File.ReadAllTextAsync(rutaFirmado, cancellationToken);
            var xmlRespuesta = File.Exists(rutaRespuesta)
                ? await File.ReadAllTextAsync(rutaRespuesta, cancellationToken)
                : null;

            var metadata = File.Exists(rutaMetadata)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(
                    await File.ReadAllTextAsync(rutaMetadata, cancellationToken))
                : new Dictionary<string, string>();

            var tamañoTotal = new FileInfo(rutaSinFirmar).Length +
                            new FileInfo(rutaFirmado).Length +
                            (xmlRespuesta != null ? new FileInfo(rutaRespuesta).Length : 0);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = File.GetCreationTime(carpetaDocumento),
                FechaActualizacion = File.GetLastWriteTime(carpetaDocumento),
                XmlSinFirmar = xmlSinFirmar,
                XmlFirmado = xmlFirmado,
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = rutaSinFirmar,
                RutaFirmado = rutaFirmado,
                RutaRespuesta = xmlRespuesta != null ? rutaRespuesta : null,
                TamañoTotalBytes = tamañoTotal,
                ProveedorStorage = ProviderName,
                MetadataAdicional = metadata ?? new Dictionary<string, string>()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error obteniendo documento {Clave}", clave);
            throw;
        }
    }

    public override async Task<bool> EliminarDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var carpetaDocumento = ObtenerCarpetaDocumento(clave);
            
            if (!Directory.Exists(carpetaDocumento))
            {
                Logger.LogWarning("Documento {Clave} no existe para eliminar", clave);
                return false;
            }

            // Soft delete: renombrar carpeta
            var carpetaEliminada = carpetaDocumento + ".deleted_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Directory.Move(carpetaDocumento, carpetaEliminada);

            Logger.LogInformation("Documento {Clave} marcado como eliminado", clave);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error eliminando documento {Clave}", clave);
            return false;
        }
    }

    public override async Task<bool> ExisteDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        var carpetaDocumento = ObtenerCarpetaDocumento(clave);
        return await Task.FromResult(Directory.Exists(carpetaDocumento));
    }

    public override async Task<List<DocumentoAlmacenado>> ListarDocumentosAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int limite = 100,
        CancellationToken cancellationToken = default)
    {
        var documentos = new List<DocumentoAlmacenado>();

        try
        {
            var carpetas = Directory.GetDirectories(_rutaActivos, "*", SearchOption.AllDirectories)
                .Where(d => EsCarpetaDocumento(d))
                .OrderByDescending(d => Directory.GetCreationTime(d))
                .Take(limite);

            foreach (var carpeta in carpetas)
            {
                var fechaCreacion = Directory.GetCreationTime(carpeta);
                
                if (fechaCreacion >= fechaDesde && fechaCreacion <= fechaHasta)
                {
                    var clave = Path.GetFileName(carpeta);
                    var doc = await ObtenerDocumentoAsync(clave, cancellationToken);
                    
                    if (doc != null)
                    {
                        documentos.Add(doc);
                    }
                }
            }

            return documentos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error listando documentos");
            throw;
        }
    }

    public override async Task<EstadisticasStorage> ObtenerEstadisticasAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var carpetas = Directory.GetDirectories(_rutaActivos, "*", SearchOption.AllDirectories)
                .Where(d => EsCarpetaDocumento(d))
                .ToList();

            long totalBytes = 0;
            foreach (var carpeta in carpetas)
            {
                var archivos = Directory.GetFiles(carpeta, "*.xml");
                totalBytes += archivos.Sum(f => new FileInfo(f).Length);
            }

            var estadistica = new EstadisticasStorage
            {
                TotalDocumentos = carpetas.Count,
                TotalBytes = totalBytes,
                FechaPrimerDocumento = carpetas.Any() 
                    ? carpetas.Min(c => Directory.GetCreationTime(c))
                    : DateTime.MinValue,
                FechaUltimoDocumento = carpetas.Any()
                    ? carpetas.Max(c => Directory.GetCreationTime(c))
                    : DateTime.MinValue
            };
            
            return await Task.FromResult(estadistica);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error obteniendo estadísticas");
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // MÉTODOS AUXILIARES
    // ═══════════════════════════════════════════════════════════

    private string CrearCarpetaDocumento(string clave)
    {
        var (año, mes, dia) = ExtraerFechaDeClave(clave);
        var carpeta = Path.Combine(
            _rutaActivos,
            año.ToString(),
            mes.ToString("D2"),
            dia.ToString("D2"),
            clave
        );

        Directory.CreateDirectory(carpeta);
        return carpeta;
    }

    private string ObtenerCarpetaDocumento(string clave)
    {
        var (año, mes, dia) = ExtraerFechaDeClave(clave);
        return Path.Combine(
            _rutaActivos,
            año.ToString(),
            mes.ToString("D2"),
            dia.ToString("D2"),
            clave
        );
    }

    private bool EsCarpetaDocumento(string carpeta)
    {
        var nombre = Path.GetFileName(carpeta);
        // Carpetas de documentos tienen 50 caracteres (la clave)
        return nombre.Length == 50 && nombre.All(char.IsDigit);
    }

    private async Task GuardarXmlAsync(XmlDocument xml, string ruta, CancellationToken cancellationToken)
    {
        using var writer = new StreamWriter(ruta, false, Encoding.UTF8);
        xml.Save(writer);
        await writer.FlushAsync();
    }

    private async Task GuardarMetadataAsync(
        string ruta,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(ruta, json, Encoding.UTF8, cancellationToken);
    }

    /// <summary>
    /// Job para comprimir documentos antiguos (ejecutar mensualmente)
    /// </summary>
    public async Task ComprimirDocumentosAntiguosAsync(CancellationToken cancellationToken = default)
    {
        if (!Options.ComprimirDespuesDeMeses)
            return;

        try
        {
            var fechaLimite = DateTime.Now.AddMonths(-Options.MesesAntesDeComprimir);
            Logger.LogInformation(
                "Comprimiendo documentos anteriores a {Fecha}",
                fechaLimite.ToString("yyyy-MM"));

            // Buscar meses completos para comprimir
            var carpetasMeses = Directory.GetDirectories(_rutaActivos, "*", SearchOption.AllDirectories)
                .Where(d => EsMesCompleto(d, fechaLimite))
                .ToList();

            foreach (var carpetaMes in carpetasMeses)
            {
                await ComprimirMesAsync(carpetaMes, cancellationToken);
            }

            Logger.LogInformation("Compresión completada. {Count} meses procesados", carpetasMeses.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error comprimiendo documentos antiguos");
        }
    }

    private bool EsMesCompleto(string carpeta, DateTime fechaLimite)
    {
        // Implementar lógica para detectar si es una carpeta de mes
        // y si está antes de la fecha límite
        return false; // Placeholder
    }

    private async Task ComprimirMesAsync(string carpetaMes, CancellationToken cancellationToken)
    {
        var nombreArchivo = Path.GetFileName(carpetaMes) + ".tar.gz";
        var rutaArchivo = Path.Combine(_rutaArchivados, nombreArchivo);

        // Comprimir usando tar.gz
        // ZipFile.CreateFromDirectory(carpetaMes, rutaArchivo, CompressionLevel.Optimal, false);

        Logger.LogInformation("Mes {Mes} comprimido en {Archivo}", carpetaMes, rutaArchivo);
        
        await Task.CompletedTask;
    }
    
    private static string DecodeBase64ToXml(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}