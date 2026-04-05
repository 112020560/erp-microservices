namespace FacturaElectronica.Dominio.Servicios.Consecutivo;

public interface IGeneradorConsecutivo
{
    string CreaNumeroSecuencia(string Sucursal, string TerminalPOS, string TipoComprobante, long NumeroFactura);
}