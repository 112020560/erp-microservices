using System.Net.Http.Headers;
using System.Text.Json;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;
using FacturaElectronica.Dominio.Exceptions;
using FacturaElectronica.Dominio.Modelos.Fiscal;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Web.Hacienda;

public class ServicioAutenticacionHacienda: IServicioAutenticacionHacienda
{
    private readonly HttpClient _httpClient;
    private readonly HaciendaSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ServicioAutenticacionHacienda> _logger;
    private const string CACHE_KEY = "hacienda_token";
    
    public ServicioAutenticacionHacienda(
        HttpClient httpClient,
        IOptions<HaciendaSettings> settings,
        IMemoryCache cache,
        ILogger<ServicioAutenticacionHacienda> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Obtener token (usa caché si está vigente)
    /// </summary>
    public async Task<string> ObtenerTokenAsync(CancellationToken cancellationToken = default)
    {
        // Intentar obtener del caché
        if (_cache.TryGetValue<ResultadoToken>(CACHE_KEY, out var tokenCacheado))
        {
            if (tokenCacheado?.EstaVigente == true)
            {
                _logger.LogDebug("Token obtenido del caché");
                return tokenCacheado.AccessToken;
            }
            
            _logger.LogDebug("Token en caché expirado");
        }
        
        // Obtener nuevo token
        var token = await ObtenerNuevoTokenAsync(cancellationToken);
        return token.AccessToken;
    }
    
    /// <summary>
    /// Obtener nuevo token de Hacienda
    /// </summary>
    public async Task<ResultadoToken> ObtenerNuevoTokenAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Obteniendo nuevo token de Hacienda ({Ambiente})", 
                _settings.Ambiente);
            
            // Preparar formulario
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _settings.ClientId,
                ["username"] = _settings.UserName,
                ["password"] = _settings.Password
            };
            
            var encodedContent = new FormUrlEncodedContent(form);
            encodedContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
            {
                CharSet = "utf-8"
            };
            
            // Crear request
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _settings.ApiOauthUrl)
            {
                Content = encodedContent
            };
            
            // Enviar request
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Manejar errores
            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<ErrorHacienda>(jsonContent);
                
                _logger.LogError(
                    "Error autenticación Hacienda: {Error} - {Description}",
                    error?.Error, 
                    error?.ErrorDescription);
                
                throw new HaciendaAuthException(
                    error?.Error ?? "unknown_error",
                    error?.ErrorDescription ?? "Error desconocido al autenticar con Hacienda");
            }
            
            // Deserializar respuesta exitosa
            var tokenResponse = JsonSerializer.Deserialize<ResultadoToken>(jsonContent);
            
            if (tokenResponse == null)
            {
                throw new HaciendaAuthException("No se pudo deserializar la respuesta del token");
            }
            
            tokenResponse.FechaObtencion = DateTime.UtcNow;
            
            // Guardar en caché
            GuardarEnCache(tokenResponse);
            
            _logger.LogInformation(
                "Token obtenido exitosamente. Expira en {Segundos} segundos",
                tokenResponse.ExpiresIn);
            
            return tokenResponse;
        }
        catch (HaciendaAuthException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de red al obtener token de Hacienda");
            throw new HaciendaAuthException($"Error de conexión con Hacienda: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener token");
            throw new HaciendaAuthException($"Error inesperado: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Invalidar token en caché (útil cuando se rechaza por expirado)
    /// </summary>
    public void InvalidarToken()
    {
        _cache.Remove(CACHE_KEY);
        _logger.LogInformation("Token invalidado del caché");
    }
    
    /// <summary>
    /// Guardar token en caché
    /// </summary>
    private void GuardarEnCache(ResultadoToken token)
    {
        // Guardar por el tiempo de expiración menos 1 minuto de margen
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(token.ExpiresIn - 60)
        };
        
        _cache.Set(CACHE_KEY, token, cacheOptions);
        
        _logger.LogDebug("Token guardado en caché por {Segundos} segundos", token.ExpiresIn - 60);
    }
}