using Amazon.S3;
using Azure.Storage.Blobs;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Settings;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Azure;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.FTP;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Google;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Local;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.S3;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Extensions;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// EXTENSIÓN PARA CONFIGURAR STORAGE (INFRAESTRUCTURA)
/// ═══════════════════════════════════════════════════════════════
/// </summary>
public static class StorageServiceExtensions
{
    public static IServiceCollection AddFacturaElectronicaStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Configurar opciones desde appsettings
    services.Configure<StorageOptions>(configuration.GetSection("Storage"));

    // 2. Registrar proveedores concretos PRIMERO
    services.AddStorageProviders(configuration);

    // 3. Registrar Factory (SIN lambda circular)
    services.AddScoped<IStorageProviderFactory>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<StorageOptions>>();
        var logger = sp.GetRequiredService<ILogger<StorageProviderFactory>>();
        
        // Crear factory directamente
        var factory = new StorageProviderFactory(sp, options, logger);
        
        // Registrar proveedores disponibles
        RegistrarProveedoresDisponibles(factory, configuration);
        
        return factory;
    });

    // 4. Registrar Facade como Scoped
    services.AddScoped<IServicioAlmacenamientoDocumentos, ServicioAlmacenamientoDocumentos>();

    return services;
        // // 1. Configurar opciones desde appsettings
        // services.Configure<StorageOptions>(configuration.GetSection("Storage"));

        // // 2. Registrar Factory como Singleton
        // services.AddSingleton<IStorageProviderFactory, StorageProviderFactory>();

        // // 3. Registrar Facade como Scoped
        // services.AddScoped<IServicioAlmacenamientoDocumentos, ServicioAlmacenamientoDocumentos>();

        // // 4. Registrar proveedores concretos
        // services.AddStorageProviders(configuration);

        // // 5. Inicializar factory con proveedores
        // services.AddSingleton(sp =>
        // {
        //     var factory = sp.GetRequiredService<IStorageProviderFactory>();
        //     RegistrarProveedoresDisponibles(factory, configuration);
        //     return factory;
        // });

        // return services;
    }

    /// <summary>
    /// Registra todos los proveedores disponibles en el factory
    /// </summary>
    private static void RegistrarProveedoresDisponibles(
        IStorageProviderFactory factory,
        IConfiguration configuration)
    {

        Console.WriteLine("=== REGISTRANDO PROVEEDORES ===");

        factory.RegistrarProveedor("FileSystem", typeof(FileSystemStorageProvider));
        // FileSystem siempre disponible
        factory.RegistrarProveedor("FileSystem", typeof(FileSystemStorageProvider));

        // AWS S3 (si está configurado)
        if (configuration.GetSection("Storage:AWS").Exists())
        {
            factory.RegistrarProveedor("AWSS3", typeof(AwsS3StorageProvider));
        }

        // Azure Blob (si está configurado)
        if (configuration.GetSection("Storage:Azure").Exists())
        {
            factory.RegistrarProveedor("AzureBlob", typeof(AzureBlobStorageProvider));
        }

        // Google Cloud (si está configurado)
        if (configuration.GetSection("Storage:GoogleCloud").Exists())
        {
            factory.RegistrarProveedor("GoogleCloud", typeof(GoogleCloudStorageProvider));
        }

        // FTP (si está configurado)
        if (configuration.GetSection("Storage:FTP").Exists())
        {
            factory.RegistrarProveedor("FTP", typeof(FtpStorageProvider));
        }
    }

    /// <summary>
    /// Registra las implementaciones concretas en DI
    /// </summary>
    private static void AddStorageProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // FileSystem (siempre disponible)
        services.AddScoped<FileSystemStorageProvider>();

        // AWS S3
        var awsConfig = configuration.GetSection("Storage:AWS");
        if (awsConfig.Exists())
        {
            services.AddAwsS3Provider(awsConfig);
        }

        // Azure Blob
        var azureConfig = configuration.GetSection("Storage:Azure");
        if (azureConfig.Exists())
        {
            services.AddAzureBlobProvider(azureConfig);
        }

        // Google Cloud
        var gcsConfig = configuration.GetSection("Storage:GoogleCloud");
        if (gcsConfig.Exists())
        {
            services.AddGoogleCloudProvider(gcsConfig);
        }

        // FTP
        var ftpConfig = configuration.GetSection("Storage:FTP");
        if (ftpConfig.Exists())
        {
            services.AddScoped<FtpStorageProvider>();
        }
    }

    private static void AddAwsS3Provider(
        this IServiceCollection services,
        IConfigurationSection config)
    {
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = config["ServiceURL"],
                ForcePathStyle = true
            };

            return new AmazonS3Client(
                config["AccessKey"],
                config["SecretKey"],
                s3Config);
        });

        services.AddScoped<AwsS3StorageProvider>();
    }

    private static void AddAzureBlobProvider(
        this IServiceCollection services,
        IConfigurationSection config)
    {
        services.AddSingleton(sp =>
        {
            var connectionString = config["ConnectionString"];
            return new BlobServiceClient(connectionString);
        });

        services.AddScoped<AzureBlobStorageProvider>();
    }

    private static void AddGoogleCloudProvider(
        this IServiceCollection services,
        IConfigurationSection config)
    {
        services.AddSingleton(sp =>
        {
            var credentialsPath = config["CredentialsPath"];
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            return StorageClient.Create();
        });

        services.AddScoped<GoogleCloudStorageProvider>();
    }
}
