namespace FacturaElectronica.Dominio.Servicios.Clave;

public interface IGeneradorClave
{
    string GenerateInvoiceKey(long numeroConsecutivo,
        string cedulaEmisor,
        DateTime fechaEmision,
        string tipoDocumento,
        string codigoSucursal,
        string situacion);

    //string GenerateCreditNoteKey(string numeroConsecutivo, string cedulaJuridica, DateTime fechaEmision);
}