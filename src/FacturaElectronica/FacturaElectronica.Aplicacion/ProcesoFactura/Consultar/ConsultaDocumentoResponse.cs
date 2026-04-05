namespace FacturaElectronica.Aplicacion.ProcesoFactura.Consultar;

public class ConsultaDocumentoResponse
{
    public string? Clave { get; set; }
    
    public string? Fecha { get; set; }
    
    public string? IndicadorEstado { get; set; }
    
    public string? RespuestaXml { get; set; }

    public ConsultaDocumentoResponse DomainToDto(Dominio.Modelos.Fiscal.ConsultaDocumentoResponse domain)
    {
        return new ConsultaDocumentoResponse()
        {
            Clave = domain.Clave,
            Fecha = domain.Fecha,
            IndicadorEstado = domain.IndicadorEstado,
            RespuestaXml = domain.RespuestaXml
        };
    }
}