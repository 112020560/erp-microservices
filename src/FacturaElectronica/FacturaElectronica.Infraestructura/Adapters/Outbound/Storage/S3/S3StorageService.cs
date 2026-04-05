using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Modelos.Storage;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.S3;

public class S3StorageService: IEstrategiaStorage
{
    private readonly ILogger<S3StorageService> _logger;
    public S3StorageService(ILogger<S3StorageService> logger)
    {
        _logger = logger;
    }
    public async Task<string> GuardarDocumentoFisicoAsync(StorageRequest request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.Content))
            throw new ArgumentException("El contenido del documento no puede estar vacío.");
        if(string.IsNullOrEmpty(request.AccessKey) || string.IsNullOrEmpty(request.SecretKey))
            throw new ArgumentException("Las credenciales de AWS no pueden estar vacías.");
        if(string.IsNullOrEmpty(request.BucketName))
            throw new ArgumentException("El nombre del bucket no puede estar vacío.");
        if(string.IsNullOrEmpty(request.DocumentName))
            throw new ArgumentException("El nombre del documento no puede estar vacío.");
        
        var credentials = new BasicAWSCredentials(request.AccessKey, request.SecretKey);
        IAmazonS3 client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);

        try
        {
            var date = DateTime.UtcNow;
            var basePath = $"/{request.TenantId}/electronic-invoice/{date:yyyy}/{date:MM}/{date:dd}/";
            var key = Path.Combine(basePath, request.DocumentName);
            
            var transferUtility = new TransferUtility(client);

            _logger.LogInformation("Starting upload...");
            
            
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = GenerateStreamFromString(request.Content),
                BucketName = request.BucketName,
                Key = key, // The name of the file in S3
                CannedACL = S3CannedACL.Private, // Set permissions as needed
                ContentType = "application/xml"
            };
            
            uploadRequest.Metadata.Add("x-amz-meta-title", "Electronic Invoice Document");
            uploadRequest.Metadata.Add("original-filename", request.DocumentName);
            // Use the high-level UploadAsync method
            await transferUtility.UploadAsync(uploadRequest, cancellationToken);

            _logger.LogInformation("Upload completed successfully!");
            
            return key; // Devuelve la ruta para guardar en DB
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogError(e,"Error uploading file: {errorMessage}", e.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown error: {errorMessage}", e.Message);
            throw;
        }
    }
    
    /// <summary>
    /// Generates a Stream from an XML string.
    /// </summary>
    /// <param name="xmlString">The XML string to convert.</param>
    /// <returns>A MemoryStream containing the XML data.</returns>
    private static Stream GenerateStreamFromString(string xmlString)
    {
        if (string.IsNullOrEmpty(xmlString))
        {
            throw new ArgumentNullException(nameof(xmlString));
        }

        // Convert the string to a byte array using UTF-8 encoding (recommended for XML)
        byte[] byteArray = Encoding.UTF8.GetBytes(xmlString);

        // Create a MemoryStream from the byte array
        var stream = new MemoryStream(byteArray);
        
        // It is good practice to set the position to 0 if the stream will be read immediately after creation
        // Although the constructor above does this automatically, it's good for general stream handling practices
        // stream.Position = 0; 

        return stream;
    }
    
    public static string BuildKey(
        string tenant,
        string module,
        string entity,
        string entityId,
        string fileName,
        string extension)
    {
        var date = DateTime.UtcNow;
        var safeFileName = SanitizeFileName(fileName);

        return $"{tenant}/{module}/{entity}/{entityId}/{date:yyyy}/{date:MM}/{date:dd}/" +
               $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}_{safeFileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');

        return fileName.Trim();
    }
}