using FacturaElectronica.Aplicacion.Abstracciones;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Abstracciones.Services;
using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.WebApi.Endpoints.Certificados;

public class CertificadosEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("certificados")
            .WithTags("Certificados");

        group.MapGet("", ObtenerCertificado)
            .WithName("ObtenerCertificadoTenant")
            .WithSummary("Obtiene la configuración del certificado del tenant actual")
            .WithOpenApi();

        group.MapPost("", ConfigurarCertificado)
            .WithName("ConfigurarCertificadoTenant")
            .WithSummary("Configura el certificado digital para el tenant actual")
            .WithOpenApi();

        group.MapDelete("", EliminarCertificado)
            .WithName("EliminarCertificadoTenant")
            .WithSummary("Elimina el certificado digital del tenant actual")
            .WithOpenApi();

        // Endpoint de utilidad para encriptar texto (útil para inserción manual en BD)
        group.MapPost("utilidades/encriptar", EncriptarTexto)
            .WithName("EncriptarTexto")
            .WithOpenApi();

        // Endpoint para verificar que una clave funciona con un certificado
        group.MapPost("utilidades/verificar", VerificarCertificado)
            .WithName("VerificarCertificado")
            .WithOpenApi();
    }

    private async Task<IResult> ObtenerCertificado(
        ITenantContext tenantContext,
        ITenantCertificateRepository repository)
    {
        var cert = await repository.GetByTenantIdAsync(tenantContext.TenantId);

        if (cert == null)
        {
            return Results.NotFound(new
            {
                TenantId = tenantContext.TenantId,
                TieneCertificado = false,
                Mensaje = "No hay certificado configurado para este tenant"
            });
        }

        return Results.Ok(new CertificadoResponse
        {
            TenantId = tenantContext.TenantId,
            TieneCertificado = true,
            NombreCertificado = cert.CertificatePath,
            FechaVencimiento = cert.ValidUntil?.ToDateTime(TimeOnly.MinValue),
            Mensaje = "Certificado configurado"
        });
    }

    private async Task<IResult> ConfigurarCertificado(
        ITenantContext tenantContext,
        ConfigurarCertificadoRequest request,
        ITenantCertificateRepository repository,
        IEncryptionService encryptionService,
        ILogger<CertificadosEndpoint> logger)
    {
        if (string.IsNullOrEmpty(request.NombreCertificado))
        {
            return Results.BadRequest(new { Error = "El nombre del certificado es requerido" });
        }

        if (string.IsNullOrEmpty(request.ClaveCertificado))
        {
            return Results.BadRequest(new { Error = "La clave del certificado es requerida" });
        }

        // Verificar que el archivo existe
        var rutaCertificado = Path.Combine(Directory.GetCurrentDirectory(), "Certificados", request.NombreCertificado);
        if (!File.Exists(rutaCertificado))
        {
            return Results.BadRequest(new
            {
                Error = $"El archivo de certificado '{request.NombreCertificado}' no existe en la carpeta Certificados/"
            });
        }

        // Encriptar la clave antes de guardar
        var claveEncriptada = encryptionService.Encrypt(request.ClaveCertificado);

        logger.LogInformation(
            "Configurando certificado {Certificado} para tenant {TenantId}",
            request.NombreCertificado, tenantContext.TenantId);

        var cert = new TenantCertificateConfig
        {
            TenantId = tenantContext.TenantId,
            CertificatePath = request.NombreCertificado,
            CertificateKeyEncrypted = claveEncriptada,
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            ValidUntil = DateOnly.FromDateTime(request.FechaVencimiento ?? DateTime.UtcNow.AddYears(2))
        };

        await repository.UpsertAsync(cert);

        return Results.Ok(new
        {
            Mensaje = "Certificado configurado exitosamente",
            TenantId = tenantContext.TenantId,
            NombreCertificado = request.NombreCertificado
        });
    }

    private async Task<IResult> EliminarCertificado(
        ITenantContext tenantContext,
        ITenantCertificateRepository repository,
        ILogger<CertificadosEndpoint> logger)
    {
        logger.LogInformation(
            "Eliminando certificado para tenant {TenantId}",
            tenantContext.TenantId);

        var resultado = await repository.DeleteAsync(tenantContext.TenantId);

        if (!resultado)
        {
            return Results.NotFound(new { Error = $"No se encontró certificado para el tenant {tenantContext.TenantId}" });
        }

        return Results.Ok(new
        {
            Mensaje = "Certificado eliminado exitosamente.",
            TenantId = tenantContext.TenantId
        });
    }

    private IResult EncriptarTexto(
        EncriptarTextoRequest request,
        IEncryptionService encryptionService)
    {
        if (string.IsNullOrEmpty(request.TextoPlano))
        {
            return Results.BadRequest(new { Error = "El texto a encriptar es requerido" });
        }

        var textoEncriptado = encryptionService.Encrypt(request.TextoPlano);

        return Results.Ok(new
        {
            TextoOriginal = request.TextoPlano,
            TextoEncriptado = textoEncriptado,
            Mensaje = "Use el valor 'TextoEncriptado' para configurar la clave del certificado"
        });
    }

    private IResult VerificarCertificado(
        VerificarCertificadoRequest request,
        ILogger<CertificadosEndpoint> logger)
    {
        if (string.IsNullOrEmpty(request.NombreCertificado))
        {
            return Results.BadRequest(new { Error = "El nombre del certificado es requerido" });
        }

        if (string.IsNullOrEmpty(request.ClaveCertificado))
        {
            return Results.BadRequest(new { Error = "La clave del certificado es requerida" });
        }

        var rutaCertificado = Path.Combine(Directory.GetCurrentDirectory(), "Certificados", request.NombreCertificado);

        if (!File.Exists(rutaCertificado))
        {
            return Results.BadRequest(new
            {
                Valido = false,
                Error = $"El archivo '{request.NombreCertificado}' no existe en la carpeta Certificados/"
            });
        }

        try
        {
            using var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                rutaCertificado,
                request.ClaveCertificado,
                System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);

            return Results.Ok(new
            {
                Valido = true,
                Subject = cert.Subject,
                Issuer = cert.Issuer,
                FechaExpiracion = cert.NotAfter,
                TieneClavePrivada = cert.HasPrivateKey,
                Mensaje = "Certificado y clave verificados correctamente"
            });
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            logger.LogWarning(ex, "Error al verificar certificado {Certificado}", request.NombreCertificado);

            return Results.Ok(new
            {
                Valido = false,
                Error = "La clave del certificado es incorrecta o el archivo está corrupto"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al verificar certificado {Certificado}", request.NombreCertificado);

            return Results.Ok(new
            {
                Valido = false,
                Error = $"Error al verificar el certificado: {ex.Message}"
            });
        }
    }
}

public class ConfigurarCertificadoRequest
{
    /// <summary>
    /// Nombre del archivo de certificado (ej: "empresa.p12").
    /// El archivo debe existir en la carpeta ./Certificados/
    /// </summary>
    public string NombreCertificado { get; set; } = string.Empty;

    /// <summary>
    /// Clave del certificado en texto plano.
    /// Se encriptará antes de almacenar en la base de datos.
    /// </summary>
    public string ClaveCertificado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de vencimiento del certificado (opcional, por defecto 2 años desde hoy).
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }
}

public class CertificadoResponse
{
    public Guid TenantId { get; set; }
    public bool TieneCertificado { get; set; }
    public string? NombreCertificado { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public class EncriptarTextoRequest
{
    /// <summary>
    /// Texto a encriptar (por ejemplo, la clave del certificado).
    /// </summary>
    public string TextoPlano { get; set; } = string.Empty;
}

public class VerificarCertificadoRequest
{
    /// <summary>
    /// Nombre del archivo de certificado (ej: "empresa.p12").
    /// </summary>
    public string NombreCertificado { get; set; } = string.Empty;

    /// <summary>
    /// Clave del certificado en texto plano.
    /// </summary>
    public string ClaveCertificado { get; set; } = string.Empty;
}
