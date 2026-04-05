namespace FacturaElectronica.Dominio.Modelos;

/// <summary>
/// Modelo de dominio para información de referencia a documentos anteriores.
/// OBLIGATORIO para Notas de Crédito y Notas de Débito.
/// </summary>
public class InformacionReferencia
{
    /// <summary>
    /// Tipo de documento referenciado:
    /// 01=Factura Electrónica, 02=Nota de Débito, 03=Nota de Crédito,
    /// 04=Tiquete Electrónico, 05=Nota de despacho, 06=Contrato,
    /// 07=Procedimiento, 08=Comprobante emitido en contingencia,
    /// 09=Sustituye comprobante rechazado, 10=Sustituye comprobante provisional,
    /// 99=Otros
    /// </summary>
    public string TipoDoc { get; set; } = "01";

    /// <summary>
    /// Número/Clave del documento referenciado (50 dígitos para documentos electrónicos)
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de emisión del documento referenciado
    /// </summary>
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Código de razón de referencia:
    /// 01=Anula documento de referencia
    /// 02=Corrige texto documento de referencia
    /// 03=Corrige monto
    /// 04=Referencia a otro documento
    /// 05=Sustituye comprobante provisional
    /// 99=Otros
    /// </summary>
    public string Codigo { get; set; } = "01";

    /// <summary>
    /// Razón de la referencia (máximo 180 caracteres)
    /// </summary>
    public string Razon { get; set; } = string.Empty;
}
