using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Consecutivo;

public class GeneradorConsecutivo : IGeneradorConsecutivo
{
    private readonly ILogger<GeneradorConsecutivo> _logger;
    public GeneradorConsecutivo(ILogger<GeneradorConsecutivo> logger)
    {
        _logger = logger;
    }
    public string CreaNumeroSecuencia(string Sucursal, string TerminalPOS, string TipoComprobante, long NumeroFactura)
    {
        /*
		Formato: SSSSSSSSSS-TT-NNNN-NNNNNNNN
		Ejemplo: 0010010001-01-0001-00000001
         |          |  |    |
         Sucursal   |  |    Consecutivo (8 dígitos)
         (10 díg)   |  Terminal (4 dígitos)  
                    Tipo Doc (2 dígitos)
		*/
        try
        {
            string consecutivo = NumeroFactura.ToString("00000000");

            if (Sucursal.Trim().Length > 3)
            {
                throw new Exception("Casa Matiz no debe de superar los 3 caracteres");
            }

            if (TerminalPOS.Trim().Length > 5)
            {
                throw new Exception("Numero de terminal no debe de superar los 5 caracteres");
            }

            if (TipoComprobante.Trim().Length > 2)
            {
                throw new Exception("Tipo Comprobante no debe de superar los 2 caracteres");
            }

            if (consecutivo.Trim().Length > 10)
            {
                throw new Exception("Numero Factura no debe de superar los 10 caracteres");
            }

            string NumeroSecuencia = "";
            NumeroSecuencia = Sucursal.Trim().PadLeft(3, '0');
            NumeroSecuencia += TerminalPOS.Trim().PadLeft(5, '0');
            NumeroSecuencia += TipoComprobante.Trim().PadLeft(2, '0');
            NumeroSecuencia += consecutivo.Trim().PadLeft(10, '0');
            if (NumeroSecuencia.Trim().Length < 20)
            {
                throw new Exception("Numero de secuencia inválido, debe tener 20 caracteres");
            }

            return NumeroSecuencia;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el numero de secuencia");
            throw;
        }

    }
}