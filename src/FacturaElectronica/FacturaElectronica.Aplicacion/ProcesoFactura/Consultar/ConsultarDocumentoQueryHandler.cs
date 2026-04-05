using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;
using FacturaElectronica.Dominio.Entidades;
using FacturaElectronica.Dominio.Modelos.Fiscal;
using FacturaElectronica.Dominio.Servicios.Factory;
using FacturaElectronica.Dominio.Servicios.Validaciones;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.Consultar;

public class ConsultarDocumentoQueryHandler: IRequestHandler<ConsultarDocumentoQuery, ConsultaDocumentoResponse>
{
    private readonly ILogger<ConsultarDocumentoQueryHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeneradorDocumentosFactory _generadorDocumentosFactory;
    private readonly IServicioDocumentosHacienda _servicioDocumentosHacienda;
    private readonly IServicioAutenticacionHacienda _servicioAutenticacionHacienda;

    public ConsultarDocumentoQueryHandler( ILogger<ConsultarDocumentoQueryHandler> logger,
        IUnitOfWork unitOfWork,
        IGeneradorDocumentosFactory generadorDocumentosFactory,
        IServicioDocumentosHacienda servicioDocumentosHacienda,
        IServicioAutenticacionHacienda servicioAutenticacionHacienda)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _generadorDocumentosFactory = generadorDocumentosFactory;
        _servicioDocumentosHacienda = servicioDocumentosHacienda;
        _servicioAutenticacionHacienda = servicioAutenticacionHacienda;
    }
    
    public async Task<ConsultaDocumentoResponse> Handle(ConsultarDocumentoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando manejo de consulta de documento con clave: {Clave}", request.Clave);
        
            var electronicInvoice = await _unitOfWork.ElectronicInvoiceRepository
                .GetByClaveAsync(request.Clave, request.TenantId, cancellationToken);
        
            if (electronicInvoice == null)
                throw new System.Exception("La factura electrónica con la clave proporcionada no existe.");
        
            var consultaResponseDomain = await _servicioDocumentosHacienda.ConsultarDocumentoAsync(request.Clave, cancellationToken);
            if(consultaResponseDomain == null)
                throw new System.Exception("No se recibió respuesta de Hacienda para la consulta del documento.");
        
            await ActualizarRegistroConRespuesta(electronicInvoice, consultaResponseDomain, new ResultadoValidacion(), cancellationToken);
            return new ConsultaDocumentoResponse().DomainToDto(consultaResponseDomain);
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Error manejando la consulta del documento con clave: {Clave}", request.Clave);
            throw;
        }
        
    }
    
    /// <summary>
    /// ═══════════════════════════════════════════════════════════
    /// PASO 2: UPDATE - Actualizar registro con respuesta de Hacienda
    /// ═══════════════════════════════════════════════════════════
    /// </summary>
    private async Task ActualizarRegistroConRespuesta(
        ElectronicInvoice registroExistente,
        Dominio.Modelos.Fiscal.ConsultaDocumentoResponse respuestaHacienda,
        ResultadoValidacion validacion,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Actualizando registro {Id} con respuesta de Hacienda", registroExistente.Id);

        var estado = DeterminarEstado(respuestaHacienda);

        // Actualizar estado
        registroExistente.Status = estado;
        registroExistente.StatusDetail = GenerarDetalleEstado(respuestaHacienda, validacion);
        
        // Fechas de respuesta
        //registroExistente.FechaRespuesta = DateTime.Now;
        
        // Respuesta de Hacienda
        registroExistente.ResponseMessage = respuestaHacienda.IndicadorEstado ?? "Sin indicador";
        
        
        // Error (si fue rechazado)
        if (estado == "rechazado" || estado == "error")
        {
            registroExistente.Error = $"{estado.ToUpper()}: {respuestaHacienda.IndicadorEstado}";
        }
        else
        {
            registroExistente.Error = null;
        }
        
        // Timestamp de actualización
        registroExistente.UpdatedAt = DateTime.Now;

        // Guardar log de respuesta
        var logRespuesta = new ElectronicDocumentLog
        {
            DocumentId = registroExistente.Id,
            Action = estado,
            Message = "Respuesta recibida de Hacienda",
            Details = $"Estado: {estado}, Indicador: {respuestaHacienda.IndicadorEstado}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ElectronicDocumentLogRepository.AddLogAsync(logRespuesta, cancellationToken);
        
        // Guardar advertencias
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
    
    
    private string DeterminarEstado(Dominio.Modelos.Fiscal.ConsultaDocumentoResponse respuesta)
    {
        if (string.IsNullOrEmpty(respuesta.IndicadorEstado))
            return "desconocido";

        return respuesta.IndicadorEstado.ToLower() switch
        {
            "aceptado" => "aceptado",
            "procesando" => "procesando",
            "rechazado" => "rechazado",
            "error" => "error",
            _ => "desconocido"
        };
    }
    
    private string GenerarDetalleEstado(Dominio.Modelos.Fiscal.ConsultaDocumentoResponse respuesta, ResultadoValidacion validacion)
    {
        var estado = respuesta.IndicadorEstado?.ToLower() ?? "desconocido";

        return estado switch
        {
            "aceptado" => $"Documento aceptado por Hacienda. Advertencias: {validacion.Advertencias.Count}",
            "procesando" => "Documento en procesamiento en Hacienda",
            "rechazado" => $"Documento rechazado por Hacienda: {respuesta.IndicadorEstado}",
            "error" => $"Error procesando documento: {respuesta.IndicadorEstado}",
            _ => $"Estado desconocido: {respuesta.IndicadorEstado}"
        };
    }
}