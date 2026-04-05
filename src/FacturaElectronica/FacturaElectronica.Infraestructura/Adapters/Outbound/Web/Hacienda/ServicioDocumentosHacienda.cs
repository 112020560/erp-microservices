using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;
using FacturaElectronica.Dominio.Modelos.Fiscal;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Web.Hacienda;

/// <summary>
/// Servicio actualizado para Hacienda v4.4
/// </summary>
public class ServicioDocumentosHacienda : IServicioDocumentosHacienda
{
    private readonly HttpClient _httpClient;
    private readonly IServicioAutenticacionHacienda _autenticacion;
    private readonly HaciendaSettings _settings;
    private readonly ILogger<ServicioDocumentosHacienda> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServicioDocumentosHacienda(
        HttpClient httpClient,
        IServicioAutenticacionHacienda autenticacion,
        IOptions<HaciendaSettings> settings,
        ILogger<ServicioDocumentosHacienda> logger)
    {
        _httpClient = httpClient;
        _autenticacion = autenticacion;
        _settings = settings.Value;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        ConfigurarHttpClient();
    }

    private void ConfigurarHttpClient()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSegundos);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Envía un documento a Hacienda para recepción (v4.4)
    /// </summary>
    public async Task<RecepcionDocumentoResponse> RecepcionDocumentoAsync(
        string token,
        RecepcionDocumentoRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando envío de documento {Clave} a Hacienda", request.Clave);

        //var urlRecepcion =_settings.ApiUrlRecepcion;

        var urlRecepcion =_settings.UsarAmbientePruebas
            ? $"{_settings.ApiUrl}/recepcion"
            : $"{_settings.ApiUrlRecepcion}/recepcion";

        var intentos = 0;
        Exception? ultimaExcepcion = null;

        while (intentos < _settings.MaxReintentos)
        {
            intentos++;

            try
            {
                var response = await EnviarConTokenAsync(urlRecepcion, request, token, cancellationToken);

                // Manejar token expirado
                if (response.StatusCode == HttpStatusCode.Unauthorized && intentos < _settings.MaxReintentos)
                {
                    _logger.LogWarning("Token expirado en intento {Intento}, obteniendo nuevo token", intentos);
                    token = await RenovarTokenAsync(cancellationToken);
                    continue;
                }

                // Manejar errores HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Error HTTP {StatusCode} enviando documento: {Error}", 
                        response.StatusCode, errorContent);
                    
                    throw new HttpRequestException(
                        $"Error {response.StatusCode} de Hacienda: {errorContent}");
                }

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    var stringContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogInformation("Respuesta Accepted de Hacienda: {Content}", stringContent);
                    if (string.IsNullOrEmpty(stringContent))
                    {
                        return new RecepcionDocumentoResponse
                        {
                            IndicadorEstado = "enviado",
                            Fecha = DateTime.UtcNow.ToShortDateString()
                        };
                    }

                    return await response.Content.ReadFromJsonAsync<RecepcionDocumentoResponse>(
                        _jsonOptions, cancellationToken) ?? new RecepcionDocumentoResponse
                    {
                        IndicadorEstado = "enviado",
                        Fecha = DateTime.UtcNow.ToShortDateString()
                    };
                }

                // Parsear respuesta exitosa
                var resultado = await response.Content.ReadFromJsonAsync<RecepcionDocumentoResponse>(
                    _jsonOptions, cancellationToken);

                if (resultado == null)
                    throw new InvalidOperationException("Respuesta de Hacienda es nula");

                _logger.LogInformation("Documento {Clave} enviado exitosamente. Estado: {Estado}", 
                    request.Clave, resultado.IndicadorEstado);

                return resultado;
            }
            catch (HttpRequestException ex)
            {
                ultimaExcepcion = ex;
                _logger.LogWarning(ex, "Error HTTP en intento {Intento} de {MaxIntentos}", 
                    intentos, _settings.MaxReintentos);

                if (intentos < _settings.MaxReintentos)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, intentos)); // Backoff exponencial
                    _logger.LogInformation("Reintentando en {Delay} segundos...", delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado enviando documento {Clave}", request.Clave);
                throw;
            }
        }

        throw new InvalidOperationException(
            $"No se pudo enviar el documento después de {_settings.MaxReintentos} intentos", 
            ultimaExcepcion);
    }

    /// <summary>
    /// Consulta el estado de un documento en Hacienda
    /// </summary>
    public async Task<ConsultaDocumentoResponse> ConsultarDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando estado de documento {Clave} en Hacienda", clave);

        var token = await _autenticacion.ObtenerTokenAsync(cancellationToken);

        var urlConsulta = $"{_settings.ApiUrlConsulta}/recepcion/{clave}";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, urlConsulta);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            // Manejar token expirado
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Token expirado, renovando para consulta");
                token = await RenovarTokenAsync(cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Respuesta de consulta: {Content}", content);

            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<ConsultaDocumentoResponse>(
                _jsonOptions, cancellationToken);

            if (resultado == null)
                throw new InvalidOperationException("Respuesta de consulta es nula");

            _logger.LogInformation("Consulta exitosa para documento {Clave}. Estado: {Estado}", 
                clave, resultado.IndicadorEstado);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando documento {Clave}", clave);
            throw;
        }
    }

    /// <summary>
    /// Envía una solicitud HTTP con el token de autorización
    /// </summary>
    private async Task<HttpResponseMessage> EnviarConTokenAsync(
        string url,
        RecepcionDocumentoRequest request,
        string token,
        CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Content = JsonContent.Create(request, options: _jsonOptions);

        _logger.LogDebug("Enviando request a {Url}", url);

        return await _httpClient.SendAsync(httpRequest, cancellationToken);
    }

    /// <summary>
    /// Renueva el token de autenticación
    /// </summary>
    private async Task<string> RenovarTokenAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Renovando token de autenticación");

        _autenticacion.InvalidarToken();
        
        var accessToken = await _autenticacion.ObtenerTokenAsync(cancellationToken);

        if (string.IsNullOrEmpty(accessToken))
            throw new InvalidOperationException("No se pudo obtener un nuevo token de acceso");

        return accessToken;
    }

    /// <summary>
    /// Valida una respuesta de Hacienda antes de procesarla
    /// </summary>
    private void ValidarRespuesta(RecepcionDocumentoResponse respuesta, string clave)
    {
        if (string.IsNullOrEmpty(respuesta.IndicadorEstado))
        {
            _logger.LogWarning("Respuesta de Hacienda para {Clave} no tiene indicador de estado", clave);
        }

        // Estados posibles en v4.4:
        // "aceptado" - Documento aceptado
        // "rechazado" - Documento rechazado
        // "procesando" - En procesamiento
        // "error" - Error en procesamiento
        
        if (respuesta.IndicadorEstado == "rechazado" || respuesta.IndicadorEstado == "error")
        {
            _logger.LogError("Documento {Clave} fue {Estado} por Hacienda", 
                clave, respuesta.IndicadorEstado);
        }
    }
}