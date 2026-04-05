using FacturaElectronica.Aplicacion.Wrappers;
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

namespace FacturaElectronica.Aplicacion.ProcesoNotaDebito.Enviar;

public class EnviarNotaDebitoCommandHandler : IRequestHandler<EnviarNotaDebitoCommand, ResultadoFacturaElectronica>
{
    private readonly ILogger<EnviarNotaDebitoCommandHandler> _logger;
    private readonly IGeneradorClave _generadorClave;
    private readonly IGeneradorConsecutivo _generadorConsecutivo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeneradorDocumentosFactory _generadorDocumentosFactory;
    private readonly IFirmaDocumentos _firmaDocumentos;
    private readonly IServicioDocumentosHacienda _servicioDocumentosHacienda;
    private readonly IServicioAutenticacionHacienda _servicioAutenticacionHacienda;
    private readonly IServicioValidacionFactura _servicioValidacion;
    private readonly ConfiguracionFacturaElectronica _configuracionFacturaElectronica;
    private readonly IServicioAlmacenamientoDocumentos _storage;
    private readonly ICertificadoProvider _certificadoProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public EnviarNotaDebitoCommandHandler(
        ILogger<EnviarNotaDebitoCommandHandler> logger,
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
        IPublishEndpoint publishEndpoint)
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
        _configuracionFacturaElectronica = options.Value;
        _storage = storage;
        _certificadoProvider = certificadoProvider;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ResultadoFacturaElectronica> Handle(EnviarNotaDebitoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando proceso de envío de nota de débito {ConsecutivoDocumento} para tenant {TenantId}",
            request.NotaDebito.ConsecutivoDocumento, request.NotaDebito.TenantId);

        ElectronicInvoice? registroDocumento = null;

        try
        {
            // 1. Validar parámetros iniciales
            ValidarParametrosIniciales(request.NotaDebito);

            // 2. Validar InformacionReferencia (OBLIGATORIA para ND)
            ValidarInformacionReferencia(request.NotaDebito.InformacionReferencia);

            // 3. Mapear a modelo de dominio
            var facturaModel = MapearRequestAFactura(request.NotaDebito);
            facturaModel.Situacion = "1"; // Normal

            // 4. Validaciones de negocio v4.4
            var resultadoValidacion = await _servicioValidacion.ValidarFacturaCompletaAsync(
                facturaModel, VersionFacturaElectronica.V44);

            if (!resultadoValidacion.EsValido)
            {
                var errores = string.Join("; ", resultadoValidacion.Errores);
                _logger.LogError("Nota de débito {ConsecutivoDocumento} contiene errores de validación: {Errores}",
                    facturaModel.ConsecutivoDocumento, errores);

                return ResultadoFacturaElectronicaExtensions.ConError(
                    "La nota de débito contiene errores de validación",
                    resultadoValidacion.Errores);
            }

            // 5. Registrar advertencias si las hay
            if (resultadoValidacion.Advertencias.Any())
            {
                var advertencias = string.Join("; ", resultadoValidacion.Advertencias);
                _logger.LogWarning("Nota de débito {ConsecutivoDocumento} contiene advertencias: {Advertencias}",
                    facturaModel.ConsecutivoDocumento, advertencias);
            }

            // 6. Generar clave y consecutivo
            var clave = _generadorClave.GenerateInvoiceKey(
                facturaModel.ConsecutivoDocumento,
                facturaModel.EmisorNumeroIdentificacion!,
                facturaModel.FechaDocumento,
                facturaModel.TipoDocumento!,
                facturaModel.Sucursal,
                facturaModel.Situacion) ??
                throw new Exception("No se pudo generar la clave de la nota de débito");

            var numeroConsecutivo = _generadorConsecutivo.CreaNumeroSecuencia(
                facturaModel.Sucursal,
                facturaModel.Terminal,
                facturaModel.TipoDocumento!,
                facturaModel.ConsecutivoDocumento) ??
                throw new Exception("No se pudo generar el consecutivo de la nota de débito");

            _logger.LogInformation("Generada clave {Clave} para nota de débito {ConsecutivoDocumento}",
                clave, facturaModel.ConsecutivoDocumento);

            // 7. Insertar registro inicial en BD
            registroDocumento = await InsertarRegistroInicial(
                request.NotaDebito.TenantId,
                request.NotaDebito.ExternalDocumentId,
                facturaModel,
                clave,
                numeroConsecutivo,
                resultadoValidacion,
                string.IsNullOrEmpty(_configuracionFacturaElectronica.CallbackUrl),
                cancellationToken);

            // 8. Generar XML
            var generadorDocumentos = _generadorDocumentosFactory.CrearGeneradorPorTipoDocumento(
                facturaModel.TipoDocumento!,
                VersionFacturaElectronica.V44);
            var xmlNotaDebitoSinFirmar = generadorDocumentos.CreaXMLFacturaElectronica(facturaModel, clave, numeroConsecutivo);

            // 9. Obtener certificado del tenant
            var certificado = await _certificadoProvider.ObtenerCertificadoAsync(
                request.NotaDebito.TenantId,
                cancellationToken);

            _logger.LogInformation(
                "Usando certificado {Certificado} para tenant {TenantId}",
                certificado.NombreCertificado,
                request.NotaDebito.TenantId);

            // 10. Firmar documento
            var resultadoFirma = _firmaDocumentos.FirmarXmlPreservandoBytes(
                xmlNotaDebitoSinFirmar,
                certificado.NombreCertificado,
                certificado.ClaveCertificado);

            // 11. Guardar documento físicamente
            await _storage.GuardarDocumentoConBytesAsync(
                clave,
                xmlNotaDebitoSinFirmar,
                resultadoFirma.BytesXmlFirmado,
                resultadoFirma.XmlDocument,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Documento XML de nota de débito guardado físicamente para clave {Clave}", clave);

            // 12. Convertir a base64 para envío
            var comprobanteXML = _firmaDocumentos.EncodeBytesToBase64(resultadoFirma.BytesXmlFirmado);

            // 13. Preparar datos de envío
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
                CallbackUrl = string.IsNullOrEmpty(_configuracionFacturaElectronica.CallbackUrl)
                    ? null
                    : _configuracionFacturaElectronica.CallbackUrl
            };

            // 14. Obtener token de acceso
            var tokenHacienda = await ObtenerTokenAccesoAsync(cancellationToken);

            // 15. Enviar documento a Hacienda
            _logger.LogInformation("Enviando nota de débito {Clave} a Hacienda v4.4", clave);
            var respuestaHacienda = await _servicioDocumentosHacienda.RecepcionDocumentoAsync(
                tokenHacienda,
                requestRecepcion,
                cancellationToken);

            // 16. Validar respuesta de Hacienda
            ValidarRespuestaHacienda(respuestaHacienda, clave);

            // 17. Guardar respuesta XML si existe
            if (!string.IsNullOrEmpty(respuestaHacienda.RespuestaXml))
            {
                await _storage.ActualizarConRespuestaAsync(
                    clave,
                    respuestaHacienda.RespuestaXml,
                    cancellationToken
                );
            }

            // 18. Actualizar registro en BD
            await ActualizarRegistroConRespuesta(registroDocumento, respuestaHacienda, resultadoValidacion, cancellationToken);

            // 19. Publicar evento de dominio
            await _publishEndpoint.Publish(new ElectronicDocumentProcessedEvent
            {
                TenantId = request.NotaDebito.TenantId,
                DocumentId = registroDocumento.Id,
                ExternalDocumentId = request.NotaDebito.ExternalDocumentId,
                DocumentType = facturaModel.TipoDocumento ?? string.Empty,
                Status = DeterminarEstado(respuestaHacienda),
                Clave = clave,
                Consecutivo = numeroConsecutivo,
                ResponseMessage = respuestaHacienda.IndicadorEstado,
                Error = null,
                ProcessedAt = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation("Nota de débito {ConsecutivoDocumento} procesada exitosamente con clave {Clave}. Estado Hacienda: {Estado}",
                facturaModel.ConsecutivoDocumento, clave, respuestaHacienda.IndicadorEstado);

            return ResultadoFacturaElectronicaExtensions.Exitoso(
                "Nota de débito procesada con éxito",
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
            _logger.LogError(ex, "Error de validación en nota de débito {ConsecutivoDocumento}",
                request.NotaDebito.ConsecutivoDocumento);
            return ResultadoFacturaElectronicaExtensions.ConError("Error de validación", [ex.Message]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando nota de débito {ConsecutivoDocumento}",
                request.NotaDebito.ConsecutivoDocumento);
            return ResultadoFacturaElectronicaExtensions.ConError("Error interno procesando nota de débito", [ex.Message]);
        }
    }

    private void ValidarParametrosIniciales(ProcesoNotaDebitoRequest notaDebito)
    {
        if (notaDebito.TenantId == Guid.Empty)
            throw new ArgumentNullException(nameof(notaDebito.TenantId), "El TenantId es requerido");

        if (string.IsNullOrEmpty(notaDebito.EmisorNumeroIdentificacion))
            throw new ArgumentNullException(nameof(notaDebito.EmisorNumeroIdentificacion),
                "La identificación del emisor es requerida");

        if (notaDebito.TipoDocumento != "02")
            throw new ArgumentException("El tipo de documento debe ser '02' para Nota de Débito");
    }

    private void ValidarInformacionReferencia(InformacionReferenciaNotaDebitoRequest? info)
    {
        if (info == null)
            throw new ArgumentException("InformacionReferencia es OBLIGATORIA para Notas de Débito");

        if (string.IsNullOrEmpty(info.Numero))
            throw new ArgumentException("El número del documento de referencia es requerido");

        if (info.Numero.Length != 50)
            throw new ArgumentException($"El número del documento de referencia debe tener 50 dígitos. Actual: {info.Numero.Length}");

        if (string.IsNullOrEmpty(info.Razon))
            throw new ArgumentException("La razón de referencia es requerida");

        if (info.Razon.Length > 180)
            throw new ArgumentException($"La razón de referencia no puede exceder 180 caracteres. Actual: {info.Razon.Length}");

        var codigosValidos = new[] { "01", "02", "03", "04", "05", "99" };
        if (!codigosValidos.Contains(info.Codigo))
            throw new ArgumentException($"Código de referencia inválido: {info.Codigo}. Válidos: {string.Join(", ", codigosValidos)}");

        var tiposDocValidos = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "99" };
        if (!tiposDocValidos.Contains(info.TipoDoc))
            throw new ArgumentException($"Tipo de documento de referencia inválido: {info.TipoDoc}");
    }

    private Factura MapearRequestAFactura(ProcesoNotaDebitoRequest request)
    {
        var factura = request.Adapt<Factura>();

        factura.InformacionReferencia = new InformacionReferencia
        {
            TipoDoc = request.InformacionReferencia.TipoDoc,
            Numero = request.InformacionReferencia.Numero,
            FechaEmision = request.InformacionReferencia.FechaEmision,
            Codigo = request.InformacionReferencia.Codigo,
            Razon = request.InformacionReferencia.Razon
        };

        return factura;
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

        var estadosValidos = new[] { "aceptado", "procesando", "rechazado", "error", "enviado" };
        if (!string.IsNullOrEmpty(respuesta.IndicadorEstado) &&
            !estadosValidos.Contains(respuesta.IndicadorEstado.ToLower()))
        {
            _logger.LogWarning("Estado desconocido de Hacienda para clave {Clave}: {Estado}",
                clave, respuesta.IndicadorEstado);
        }

        switch (respuesta.IndicadorEstado?.ToLower())
        {
            case "aceptado":
                _logger.LogInformation("Nota de débito {Clave} ACEPTADA por Hacienda", clave);
                break;
            case "procesando":
                _logger.LogInformation("Nota de débito {Clave} en PROCESAMIENTO en Hacienda", clave);
                break;
            case "enviado":
                _logger.LogInformation("Nota de débito {Clave} ENVIADA a Hacienda", clave);
                break;
            case "rechazado":
                _logger.LogError("Nota de débito {Clave} RECHAZADA por Hacienda", clave);
                throw new InvalidOperationException($"Nota de débito rechazada por Hacienda: {clave}");
            case "error":
                _logger.LogError("ERROR procesando nota de débito {Clave} en Hacienda", clave);
                throw new InvalidOperationException($"Error en Hacienda procesando nota de débito: {clave}");
        }
    }

    private async Task<ElectronicInvoice> InsertarRegistroInicial(
        Guid tenantId,
        string? externalDocumentId,
        Factura factura,
        string clave,
        string consecutivo,
        ResultadoValidacion validacion,
        bool isCallback,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Insertando registro inicial en BD para nota de débito con clave {Clave}", clave);

        var entidad = new ElectronicInvoice
        {
            TenantId = tenantId,
            ExternalDocumentId = externalDocumentId,
            InvoiceType = factura.TipoDocumento!,
            Status = "procesando",
            StatusDetail = "Nota de débito preparada - Enviando a Hacienda...",
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
            ProcessType = isCallback ? "callback" : "polling",
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
            Message = "Nota de débito preparada y enviando",
            Details = $"Versión: 4.4, Advertencias: {validacion.Advertencias.Count}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ElectronicDocumentLogRepository.AddLogAsync(logInicial, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registro inicial insertado con Id {Id} para nota de débito clave {Clave}", entidad.Id, clave);

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
            registroExistente.Error = $"{estado.ToUpper()}: {respuestaHacienda.IndicadorEstado}";
        else
            registroExistente.Error = null;

        registroExistente.UpdatedAt = DateTime.UtcNow;

        var logRespuesta = new ElectronicDocumentLog
        {
            DocumentId = registroExistente.Id,
            Action = estado,
            Message = "Respuesta recibida de Hacienda para nota de débito",
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
            "aceptado" => $"Nota de débito aceptada por Hacienda. Advertencias: {validacion.Advertencias.Count}",
            "procesando" => "Nota de débito en procesamiento en Hacienda",
            "rechazado" => $"Nota de débito rechazada por Hacienda: {respuesta.IndicadorEstado}",
            "error" => $"Error procesando nota de débito: {respuesta.IndicadorEstado}",
            "enviado" => "Nota de débito enviada a Hacienda",
            _ => $"Estado desconocido: {respuesta.IndicadorEstado}"
        };
    }
}
