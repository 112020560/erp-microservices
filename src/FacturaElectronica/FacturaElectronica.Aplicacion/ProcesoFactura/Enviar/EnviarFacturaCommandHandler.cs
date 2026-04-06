using FacturaElectronica.Aplicacion.Wrappers;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;
using FacturaElectronica.Dominio.Abstracciones.Services;
using FacturaElectronica.Dominio.Entidades;
using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Modelos.Fiscal;
using FacturaElectronica.Dominio.Servicios.Clave;
using FacturaElectronica.Dominio.Servicios.Consecutivo;
using FacturaElectronica.Dominio.Servicios.Documentos.Firmas;
using FacturaElectronica.Dominio.Servicios.Factory;
using FacturaElectronica.Dominio.Servicios.Validaciones;
using FacturaElectronica.Dominio.Servicios.Validaciones.Facturas;
using FacturaElectronica.Dominio.Settings;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.Enviar;

public class EnviarFacturaCommandHandler : IRequestHandler<EnviarFacturaCommand, ResultadoFacturaElectronica>
{
    private readonly ILogger<EnviarFacturaCommandHandler> _logger;
    private readonly IGeneradorClave _generadorClave;
    private readonly IGeneradorConsecutivo _generadorConsecutivo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeneradorDocumentosFactory _generadorDocumentosFactory;
    private readonly IFirmaDocumentos _firmaDocumentos;
    private readonly IServicioDocumentosHacienda _servicioDocumentosHacienda;
    private readonly IServicioAutenticacionHacienda _servicioAutenticacionHacienda;
    private readonly IServicioValidacionFactura _servicioValidacion;
    private readonly ConfiguracionFacturaElectronica configuracionFacturaElectronica;
    private readonly IServicioAlmacenamientoDocumentos _storage;
    private readonly ICertificadoProvider _certificadoProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IWebhookDispatcherService _webhookDispatcher;
    private readonly ITenantNotificationConfigRepository _notificationConfigRepository;

    public EnviarFacturaCommandHandler(
        ILogger<EnviarFacturaCommandHandler> logger,
        IGeneradorClave generadorClave,
        IGeneradorConsecutivo generadorConsecutivo,
        IUnitOfWork unitOfWork,
        IGeneradorDocumentosFactory generadorDocumentosFactory,
        IFirmaDocumentos firmaDocumentos,
        IServicioAutenticacionHacienda servicioAutenticacionHacienda,
        IServicioDocumentosHacienda servicioDocumentosHacienda,
        IServicioValidacionFactura servicioValidacion,
        IServicioAlmacenamientoDocumentos storage,
        IOptions<ConfiguracionFacturaElectronica> options,
        ICertificadoProvider certificadoProvider,
        IPublishEndpoint publishEndpoint,
        IWebhookDispatcherService webhookDispatcher,
        ITenantNotificationConfigRepository notificationConfigRepository)
    {
        _logger = logger;
        _generadorClave = generadorClave;
        _generadorConsecutivo = generadorConsecutivo;
        _unitOfWork = unitOfWork;
        _generadorDocumentosFactory = generadorDocumentosFactory;
        _firmaDocumentos = firmaDocumentos;
        _servicioDocumentosHacienda = servicioDocumentosHacienda;
        _servicioAutenticacionHacienda = servicioAutenticacionHacienda;
        _servicioValidacion = servicioValidacion;
        configuracionFacturaElectronica = options.Value;
        _storage = storage;
        _certificadoProvider = certificadoProvider;
        _publishEndpoint = publishEndpoint;
        _webhookDispatcher = webhookDispatcher;
        _notificationConfigRepository = notificationConfigRepository;
    }

    public async Task<ResultadoFacturaElectronica> Handle(EnviarFacturaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando proceso de envío de factura {ConsecutivoDocumento} para tenant {TenantId}",
            request.Factura.ConsecutivoDocumento, request.Factura.TenantId);

        ElectronicInvoice? registroDocumento = null;

        try
        {
            // 1. Validaciones básicas iniciales
            ValidarParametrosIniciales(request.Factura);

            // 2. Mapear a modelo de dominio
            var facturaModel = request.Factura.Adapt<Factura>();
            facturaModel.Situacion = "1"; // Normal

            // 3. VALIDACIONES DE NEGOCIO V4.4
            var resultadoValidacion = await _servicioValidacion.ValidarFacturaCompletaAsync(facturaModel, VersionFacturaElectronica.V44);

            if (!resultadoValidacion.EsValido)
            {
                var errores = string.Join("; ", resultadoValidacion.Errores);
                _logger.LogError("Factura {ConsecutivoDocumento} contiene errores de validación: {Errores}",
                    facturaModel.ConsecutivoDocumento, errores);

                return ResultadoFacturaElectronicaExtensions.ConError(
                    "La factura contiene errores de validación",
                    resultadoValidacion.Errores);
            }

            // 4. Registrar advertencias si las hay
            if (resultadoValidacion.Advertencias.Any())
            {
                var advertencias = string.Join("; ", resultadoValidacion.Advertencias);
                _logger.LogWarning("Factura {ConsecutivoDocumento} contiene advertencias: {Advertencias}",
                    facturaModel.ConsecutivoDocumento, advertencias);
            }

            // 5. Generar clave y consecutivo
            var clave = _generadorClave.GenerateInvoiceKey(
                facturaModel.ConsecutivoDocumento,
                facturaModel.EmisorNumeroIdentificacion!,
                facturaModel.FechaDocumento,
                facturaModel.TipoDocumento!,
                facturaModel.Sucursal,
                facturaModel.Situacion) ??
                throw new Exception("No se pudo generar la clave de la factura");

            var numeroConsecutivo = _generadorConsecutivo.CreaNumeroSecuencia(
                facturaModel.Sucursal,
                facturaModel.Terminal,
                facturaModel.TipoDocumento!,
                facturaModel.ConsecutivoDocumento) ??
                throw new Exception("No se pudo generar el consecutivo de la factura");

            _logger.LogInformation("Generada clave {Clave} para factura {ConsecutivoDocumento}", clave, facturaModel.ConsecutivoDocumento);

            // 6. Insertar registro inicial en BD
            registroDocumento = await InsertarRegistroInicial(
                request.Factura.TenantId,
                request.Factura.ExternalDocumentId,
                facturaModel,
                clave,
                numeroConsecutivo,
                resultadoValidacion,
                string.IsNullOrEmpty(configuracionFacturaElectronica.CallbackUrl),
                cancellationToken);

            // 7. Generar XML
            var generadorDocumentos = _generadorDocumentosFactory.CrearGeneradorPorTipoDocumento(
                facturaModel.TipoDocumento!,
                VersionFacturaElectronica.V44);
            var xmlFacturaSinFirmar = generadorDocumentos.CreaXMLFacturaElectronica(facturaModel, clave, numeroConsecutivo);

            // 8. Obtener certificado del tenant
            var certificado = await _certificadoProvider.ObtenerCertificadoAsync(
                request.Factura.TenantId,
                cancellationToken);

            _logger.LogInformation(
                "Usando certificado {Certificado} para tenant {TenantId}",
                certificado.NombreCertificado,
                request.Factura.TenantId);

            // 9. Firmar documento
            var resultadoFirma = _firmaDocumentos.FirmarXmlPreservandoBytes(
                xmlFacturaSinFirmar,
                certificado.NombreCertificado,
                certificado.ClaveCertificado);

            // 10. Guardar documento físicamente
            await _storage.GuardarDocumentoConBytesAsync(
                clave,
                xmlFacturaSinFirmar,
                resultadoFirma.BytesXmlFirmado,
                resultadoFirma.XmlDocument,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Documento XML guardado físicamente para clave {Clave}", clave);

            // 11. Convertir a base64 para envío
            var comprobanteXML = _firmaDocumentos.EncodeBytesToBase64(resultadoFirma.BytesXmlFirmado);

            // 12. Preparar datos de envío
            var requestRecepcion = new RecepcionDocumentoRequest
            {
                Clave = clave,
                Fecha = facturaModel.FechaDocumento.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Emisor = new EmisorRequest
                {
                    TipoIdentificacion = facturaModel.EmisorTipoIdentificacion ?? string.Empty,
                    NumeroIdentificacion = facturaModel.EmisorNumeroIdentificacion ?? string.Empty
                },
                Receptor = facturaModel.Receptor && !string.IsNullOrEmpty(facturaModel.ReceptorNumeroIdentificacion)
                    ? new ReceptorRequest
                    {
                        TipoIdentificacion = facturaModel.ReceptorTipoIdentificacion,
                        NumeroIdentificacion = facturaModel.ReceptorNumeroIdentificacion
                    }
                    : null,
                ComprobanteXml = comprobanteXML,
                CallbackUrl = string.IsNullOrEmpty(configuracionFacturaElectronica.CallbackUrl)
                    ? null
                    : configuracionFacturaElectronica.CallbackUrl
            };

            // 13. Obtener token de acceso
            var tokenHacienda = await ObtenerTokenAccesoAsync(cancellationToken);

            // 14. Enviar documento a Hacienda
            _logger.LogInformation("Enviando factura {Clave} a Hacienda v4.4", clave);
            var respuestaHacienda = await _servicioDocumentosHacienda.RecepcionDocumentoAsync(
                tokenHacienda,
                requestRecepcion,
                cancellationToken);

            // 15. Validar respuesta de Hacienda
            ValidarRespuestaHacienda(respuestaHacienda, clave);

            // 16. Guardar respuesta XML si existe
            if (!string.IsNullOrEmpty(respuestaHacienda.RespuestaXml))
            {
                await _storage.ActualizarConRespuestaAsync(
                    clave,
                    respuestaHacienda.RespuestaXml,
                    cancellationToken
                );
            }

            // 17. Actualizar registro en BD
            await ActualizarRegistroConRespuesta(registroDocumento, respuestaHacienda, resultadoValidacion, cancellationToken);

            // 18. Load notification config and route accordingly
            var notificationConfig = await _notificationConfigRepository.GetByTenantIdAsync(
                request.Factura.TenantId, cancellationToken);

            var channel = notificationConfig?.Channel ?? NotificationChannel.None;

            if (channel.HasFlag(NotificationChannel.RabbitMq))
            {
                await _publishEndpoint.Publish(new ElectronicDocumentProcessedEvent
                {
                    TenantId = request.Factura.TenantId,
                    DocumentId = registroDocumento.Id,
                    ExternalDocumentId = request.Factura.ExternalDocumentId,
                    DocumentType = facturaModel.TipoDocumento ?? string.Empty,
                    Status = DeterminarEstado(respuestaHacienda),
                    Clave = clave,
                    Consecutivo = numeroConsecutivo,
                    ResponseMessage = respuestaHacienda.IndicadorEstado,
                    Error = null,
                    ProcessedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            // Webhook is handled internally by the dispatcher (checks config.Channel)
            await _webhookDispatcher.DispatchAsync(
                request.Factura.TenantId,
                "document.processed",
                new
                {
                    DocumentId = registroDocumento.Id,
                    ExternalDocumentId = request.Factura.ExternalDocumentId,
                    DocumentType = facturaModel.TipoDocumento,
                    Status = DeterminarEstado(respuestaHacienda),
                    Clave = clave,
                    Consecutivo = numeroConsecutivo,
                    ProcessedAt = DateTime.UtcNow
                },
                cancellationToken);

            _logger.LogInformation("Factura {ConsecutivoDocumento} procesada exitosamente con clave {Clave}. Estado Hacienda: {Estado}",
                facturaModel.ConsecutivoDocumento, clave, respuestaHacienda.IndicadorEstado);

            return ResultadoFacturaElectronicaExtensions.Exitoso(
                "Factura procesada con éxito",
                new DatosFacturaElectronica
                {
                    Clave = clave,
                    NumeroConsecutivo = numeroConsecutivo,
                    EstadoHacienda = respuestaHacienda.IndicadorEstado ?? "enviado",
                    Advertencias = resultadoValidacion.Advertencias
                });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación en factura {ConsecutivoDocumento}", request.Factura.ConsecutivoDocumento);
            return ResultadoFacturaElectronicaExtensions.ConError("Error de validación", [ex.Message]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando factura {ConsecutivoDocumento}", request.Factura.ConsecutivoDocumento);

            if (registroDocumento is not null)
            {
                try
                {
                    var errorConfig = await _notificationConfigRepository.GetByTenantIdAsync(
                        request.Factura.TenantId, cancellationToken);
                    var errorChannel = errorConfig?.Channel ?? NotificationChannel.None;

                    if (errorChannel.HasFlag(NotificationChannel.RabbitMq))
                    {
                        await _publishEndpoint.Publish(new ElectronicDocumentProcessedEvent
                        {
                            TenantId = request.Factura.TenantId,
                            DocumentId = registroDocumento.Id,
                            ExternalDocumentId = request.Factura.ExternalDocumentId,
                            DocumentType = request.Factura.TipoDocumento ?? string.Empty,
                            Status = "error",
                            Error = ex.Message,
                            ProcessedAt = DateTime.UtcNow
                        }, cancellationToken);
                    }
                }
                catch (Exception publishEx)
                {
                    _logger.LogError(publishEx, "Error publicando evento de error para factura");
                }
            }

            return ResultadoFacturaElectronicaExtensions.ConError("Error interno procesando factura", [ex.Message]);
        }
    }

    private void ValidarParametrosIniciales(ProcesoFacturaRequest factura)
    {
        if (factura.TenantId == Guid.Empty)
            throw new ArgumentNullException(nameof(factura.TenantId), "El TenantId es requerido");

        if (string.IsNullOrEmpty(factura.EmisorNumeroIdentificacion))
            throw new ArgumentNullException(nameof(factura.EmisorNumeroIdentificacion), "La identificación del emisor es requerida");

        if (string.IsNullOrEmpty(factura.TipoDocumento))
            throw new ArgumentNullException(nameof(factura.TipoDocumento), "El tipo de documento es requerido");
    }

    private async Task<string> ObtenerTokenAccesoAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo token de acceso de Hacienda");

        var tokenHacienda = await _servicioAutenticacionHacienda.ObtenerTokenAsync(
            cancellationToken
        ) ?? throw new InvalidOperationException("No se pudo obtener el token de acceso");

        if (string.IsNullOrEmpty(tokenHacienda))
            throw new InvalidOperationException("Token de acceso nulo o vacío");

        _logger.LogDebug("Token de acceso obtenido exitosamente");
        return tokenHacienda;
    }

    private void ValidarRespuestaHacienda(RecepcionDocumentoResponse respuesta, string clave)
    {
        if (respuesta == null)
            throw new InvalidOperationException($"Respuesta de Hacienda es nula para clave {clave}");

        if (string.IsNullOrEmpty(respuesta.IndicadorEstado))
            _logger.LogWarning("Respuesta de Hacienda no incluye indicador de estado para clave {Clave}", clave);

        var estadosValidos = new[] { "aceptado", "procesando", "rechazado", "error" };
        if (!string.IsNullOrEmpty(respuesta.IndicadorEstado) &&
            !estadosValidos.Contains(respuesta.IndicadorEstado.ToLower()))
        {
            _logger.LogWarning("Estado desconocido de Hacienda para clave {Clave}: {Estado}",
                clave, respuesta.IndicadorEstado);
        }

        switch (respuesta.IndicadorEstado?.ToLower())
        {
            case "aceptado":
                _logger.LogInformation("Documento {Clave} ACEPTADO por Hacienda", clave);
                break;
            case "procesando":
                _logger.LogInformation("Documento {Clave} en PROCESAMIENTO en Hacienda", clave);
                break;
            case "enviado":
                _logger.LogInformation("Documento {Clave} en ENVIADO en Hacienda", clave);
                break;
            case "rechazado":
                _logger.LogError("Documento {Clave} RECHAZADO por Hacienda", clave);
                throw new InvalidOperationException($"Documento rechazado por Hacienda: {clave}");
            case "error":
                _logger.LogError("ERROR procesando documento {Clave} en Hacienda", clave);
                throw new InvalidOperationException($"Error en Hacienda procesando documento: {clave}");
        }
    }

    private async Task<ElectronicInvoice> InsertarRegistroInicial(
        Guid tenantId,
        string? externalDocumentId,
        Factura factura,
        string clave,
        string consecutivo,
        ResultadoValidacion validacion,
        bool IsCallback,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Insertando registro inicial en BD para clave {Clave}", clave);

        var entidad = new ElectronicInvoice
        {
            TenantId = tenantId,
            ExternalDocumentId = externalDocumentId,
            InvoiceType = factura.TipoDocumento!,
            Status = "procesando",
            StatusDetail = "Documento preparado - Enviando a Hacienda...",
            Clave = clave,
            Consecutivo = consecutivo,
            EmisorIdentificacion = factura.EmisorNumeroIdentificacion,
            ReceptorIdentificacion = factura.ReceptorNumeroIdentificacion,
            XmlEmisorPath = $"xml/enviados/{clave}.xml",
            XmlReceptorPath = null,
            FechaEmision = factura.FechaDocumento,
            FechaEnvio = DateTime.UtcNow,
            FechaRespuesta = null,
            ResponseMessage = null,
            Error = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ProcessType = IsCallback ? "callback" : "polling",
            NombreReceptor = factura.ReceptorNombre,
            CorreoReceptor = factura.ReceptorCorreoElectronico,
            TelefonoReceptor = factura.ReceptorTelefono,
            NotificacionEnviada = false
        };

        await _unitOfWork.ElectronicInvoiceRepository.AddAsync(entidad, cancellationToken);

        var logInicial = new ElectronicDocumentLog
        {
            DocumentId = entidad.Id,
            Action = "procesando",
            Message = "Documento preparado y enviando",
            Details = $"Versión: 4.4, Advertencias: {validacion.Advertencias.Count}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ElectronicDocumentLogRepository.AddLogAsync(logInicial, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registro inicial insertado con Id {Id} para clave {Clave}", entidad.Id, clave);

        return entidad;
    }

    private async Task ActualizarRegistroConRespuesta(
        ElectronicInvoice registroExistente,
        RecepcionDocumentoResponse respuestaHacienda,
        ResultadoValidacion validacion,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Actualizando registro {Id} con respuesta de Hacienda", registroExistente.Id);

        var estado = DeterminarEstado(respuestaHacienda);

        registroExistente.Status = estado;
        registroExistente.StatusDetail = GenerarDetalleEstado(respuestaHacienda, validacion);
        registroExistente.ResponseMessage = respuestaHacienda.IndicadorEstado ?? "Sin indicador";

        if (estado == "rechazado" || estado == "error")
        {
            registroExistente.Error = $"{estado.ToUpper()}: {respuestaHacienda.IndicadorEstado}";
        }
        else
        {
            registroExistente.Error = null;
        }

        registroExistente.UpdatedAt = DateTime.UtcNow;

        var logRespuesta = new ElectronicDocumentLog
        {
            DocumentId = registroExistente.Id,
            Action = estado,
            Message = "Respuesta recibida de Hacienda",
            Details = $"Estado: {estado}, Indicador: {respuestaHacienda.IndicadorEstado}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ElectronicDocumentLogRepository.AddLogAsync(logRespuesta, cancellationToken);

        if (validacion.Advertencias.Any())
        {
            foreach (var advertencia in validacion.Advertencias)
            {
                var logAdvertencia = new ElectronicDocumentLog
                {
                    DocumentId = registroExistente.Id,
                    Action = "advertencia",
                    Message = "Advertencia de validación",
                    Details = advertencia,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ElectronicDocumentLogRepository.AddLogAsync(logAdvertencia, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registro {Id} actualizado exitosamente con estado {Estado}",
            registroExistente.Id, estado);
    }

    private string DeterminarEstado(RecepcionDocumentoResponse respuesta)
    {
        if (string.IsNullOrEmpty(respuesta.IndicadorEstado))
            return "desconocido";

        return respuesta.IndicadorEstado.ToLower() switch
        {
            "aceptado" => "aceptado",
            "procesando" => "procesando",
            "rechazado" => "rechazado",
            "enviado" => "enviado",
            "error" => "error",
            _ => "desconocido"
        };
    }

    private string GenerarDetalleEstado(RecepcionDocumentoResponse respuesta, ResultadoValidacion validacion)
    {
        var estado = respuesta.IndicadorEstado?.ToLower() ?? "desconocido";

        return estado switch
        {
            "aceptado" => $"Documento aceptado por Hacienda. Advertencias: {validacion.Advertencias.Count}",
            "procesando" => "Documento en procesamiento en Hacienda",
            "rechazado" => $"Documento rechazado por Hacienda: {respuesta.IndicadorEstado}",
            "error" => $"Error procesando documento: {respuesta.IndicadorEstado}",
            "enviado" => "Documento enviado a Hacienda",
            _ => $"Estado desconocido: {respuesta.IndicadorEstado}"
        };
    }
}
