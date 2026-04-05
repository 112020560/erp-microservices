using System.Xml;

namespace FacturaElectronica.Dominio.Servicios.Documentos.Firmas;

/// <summary>
/// Resultado de la firma que preserva los bytes originales del XML firmado
/// para evitar problemas de re-serialización que invalidan la firma digital
/// </summary>
public class ResultadoFirma
{
    /// <summary>
    /// Bytes exactos del XML firmado - USAR ESTOS para enviar a Hacienda y guardar
    /// </summary>
    public byte[] BytesXmlFirmado { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// String del XML firmado (de los bytes originales)
    /// </summary>
    public string XmlFirmadoString => System.Text.Encoding.UTF8.GetString(BytesXmlFirmado);

    /// <summary>
    /// XmlDocument para lectura/metadata (NO usar para serializar de nuevo)
    /// </summary>
    public XmlDocument XmlDocument { get; set; } = new XmlDocument();
}

public interface IFirmaDocumentos
{
    // ═══════════════════════════════════════════════════════════════
    // MÉTODO ANTERIOR (comentado para referencia)
    // ═══════════════════════════════════════════════════════════════
    // XmlDocument FirmarXml(XmlDocument xmlDoc, string? pathCertificado, string? passwordCertificado);

    /// <summary>
    /// Firma el XML y devuelve un ResultadoFirma que preserva los bytes exactos.
    /// IMPORTANTE: Usar BytesXmlFirmado para enviar a Hacienda, NO re-serializar el XmlDocument.
    /// </summary>
    ResultadoFirma FirmarXmlPreservandoBytes(XmlDocument xmlDoc, string? pathCertificado, string? passwordCertificado);

    /// <summary>
    /// [DEPRECADO] Este método puede causar problemas de firma inválida.
    /// Usar FirmarXmlPreservandoBytes en su lugar.
    /// </summary>
    [Obsolete("Usar FirmarXmlPreservandoBytes para evitar problemas de re-serialización")]
    XmlDocument FirmarXml(XmlDocument xmlDoc, string? pathCertificado, string? passwordCertificado);

    XmlDocument DecodeBase64ToXML(string valor);
    string DecodeBase64ToString(string valor);
    string EncodeStrToBase64(string valor);

    /// <summary>
    /// Codifica bytes a Base64 (preferir sobre EncodeStrToBase64 para XML firmado)
    /// </summary>
    string EncodeBytesToBase64(byte[] bytes);
}