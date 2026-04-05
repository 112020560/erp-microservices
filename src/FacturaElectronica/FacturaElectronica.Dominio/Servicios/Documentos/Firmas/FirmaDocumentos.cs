using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using FirmaXadesNetCore;
using FirmaXadesNetCore.Signature.Parameters;

namespace FacturaElectronica.Dominio.Servicios.Documentos.Firmas;

public class FirmaDocumentos : IFirmaDocumentos
{
    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Preserva los bytes exactos del XML firmado
    /// ═══════════════════════════════════════════════════════════════
    /// Este método soluciona el problema de "XML modificado después de firmado"
    /// al preservar los bytes exactos que salen del proceso de firma.
    /// </summary>
    public ResultadoFirma FirmarXmlPreservandoBytes(XmlDocument xmlDoc, string? nombreCertificado, string? passwordCertificado)
    {
        if (nombreCertificado == null || passwordCertificado == null)
        {
            throw new Exception("No se ha configurado el certificado");
        }

        var directory = Directory.GetCurrentDirectory();
        var path = Path.Combine(directory, "Certificados", nombreCertificado);

        // 1. Cargar el certificado desde archivo
        var cert = LoadCertificate(path, passwordCertificado);

        // 2. Configurar los parámetros de firma
        var xadesService = new XadesService();
        var parametros = new SignatureParameters
        {
            SignaturePolicyInfo = new SignaturePolicyInfo
            {
                PolicyIdentifier = "https://tribunet.hacienda.go.cr/docs/esquemas/2016/v4.1/Resolucion_Comprobantes_Electronicos_DGT-R-48-2016.pdf",
                PolicyHash = "Ohixl6upD6av8N7pEvDABhEL6hM="
            },
            SignaturePackaging = SignaturePackaging.ENVELOPED,
            DataFormat = new DataFormat(),
            Signer = new FirmaXadesNetCore.Crypto.Signer(cert)
        };

        // 3. Convertir el XmlDocument en stream
        using var ms = new MemoryStream();
        xmlDoc.Save(ms);
        ms.Position = 0;

        // 4. Firmar el XML
        var docFirmado = xadesService.Sign(ms, parametros);

        // 5. ═══════════════════════════════════════════════════════════════
        //    CRÍTICO: Obtener los bytes EXACTOS del documento firmado
        //    NO re-serializar con XmlDocument.OuterXml después de esto
        // ═══════════════════════════════════════════════════════════════
        using var msOut = new MemoryStream();
        docFirmado.Save(msOut);
        var bytesExactos = msOut.ToArray();

        // 6. Cargar en XmlDocument solo para lectura/metadata (NO para re-serializar)
        var xmlFirmado = new XmlDocument();
        using var msRead = new MemoryStream(bytesExactos);
        xmlFirmado.Load(msRead);

        return new ResultadoFirma
        {
            BytesXmlFirmado = bytesExactos,
            XmlDocument = xmlFirmado
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // MÉTODO ANTERIOR (mantenido por compatibilidad, marcado obsoleto)
    // ═══════════════════════════════════════════════════════════════
    // PROBLEMA: Este método devuelve XmlDocument, y cuando se usa
    // XmlDocument.OuterXml para serializar, puede cambiar el formato
    // del XML e invalidar la firma digital.
    // ═══════════════════════════════════════════════════════════════
    [Obsolete("Usar FirmarXmlPreservandoBytes para evitar problemas de re-serialización")]
    public XmlDocument FirmarXml(XmlDocument xmlDoc, string? nombreCertificado, string? passwordCertificado)
    {
        // Usar el nuevo método internamente
        var resultado = FirmarXmlPreservandoBytes(xmlDoc, nombreCertificado, passwordCertificado);
        return resultado.XmlDocument;

        // ═══════════════════════════════════════════════════════════════
        // CÓDIGO ANTERIOR (comentado para referencia):
        // ═══════════════════════════════════════════════════════════════
        /*
        if (nombreCertificado == null || passwordCertificado == null)
        {
            throw new Exception("No se ha configurado el certificado");
        }

        var directory = Directory.GetCurrentDirectory();
        var path = Path.Combine(directory, "Certificados", nombreCertificado);
        // 1. Cargar el certificado desde archivo
        var cert = LoadCertificate(path, passwordCertificado);

        // 2. Configurar los parámetros de firma
        var xadesService = new XadesService();
        var parametros = new SignatureParameters
        {
            SignaturePolicyInfo = new SignaturePolicyInfo
            {
                PolicyIdentifier = "https://tribunet.hacienda.go.cr/docs/esquemas/2016/v4.1/Resolucion_Comprobantes_Electronicos_DGT-R-48-2016.pdf",
                PolicyHash = "Ohixl6upD6av8N7pEvDABhEL6hM="
            },
            SignaturePackaging = SignaturePackaging.ENVELOPED,
            DataFormat = new DataFormat(),
            Signer = new FirmaXadesNetCore.Crypto.Signer(cert)
        };

        // 3. Convertir el XmlDocument en stream
        using var ms = new MemoryStream();
        xmlDoc.Save(ms);
        ms.Position = 0;

        // 4. Firmar el XML
        var docFirmado = xadesService.Sign(ms, parametros);

        // 5. Convertir el firmado a XmlDocument de vuelta
        // ⚠️ PROBLEMA: Al usar XmlDocument.Save() y luego OuterXml,
        //              el formato puede cambiar e invalidar la firma
        var xmlFirmado = new XmlDocument();
        using var msOut = new MemoryStream();
        docFirmado.Save(msOut);
        msOut.Position = 0;
        xmlFirmado.Load(msOut);

        return xmlFirmado;
        */
    }

    public XmlDocument DecodeBase64ToXML(string valor)
    {
        byte[] myBase64ret = Convert.FromBase64String(valor);
        string myStr = System.Text.Encoding.UTF8.GetString(myBase64ret);
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(myStr);
        return xmlDoc;
    }

    public string DecodeBase64ToString(string valor)
    {
        byte[] myBase64ret = Convert.FromBase64String(valor);
        string myStr = System.Text.Encoding.UTF8.GetString(myBase64ret);
        return myStr;
    }

    public string EncodeStrToBase64(string valor)
    {
        byte[] myByte = System.Text.Encoding.UTF8.GetBytes(valor);
        string myBase64 = Convert.ToBase64String(myByte);
        return myBase64;
    }

    /// <summary>
    /// Codifica bytes directamente a Base64.
    /// Usar este método para el XML firmado para evitar re-serialización.
    /// </summary>
    public string EncodeBytesToBase64(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    private X509Certificate2 LoadCertificate(string certPath, string certPassword)
    {
        return new X509Certificate2(
            certPath,
            certPassword,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
    }
    
    // public X509Certificate2 GetCertificateByThumbprint(string thumbprintCertificado)
    //     {
    //         X509Certificate2 cert = null;
    //         X509Store store = new X509Store("My", StoreLocation.CurrentUser);
    //         try
    //         {
    //             store.Open((OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly));
    //             X509Certificate2Collection CertCol = store.Certificates;
    //             foreach (X509Certificate2 c in CertCol)
    //             {
    //                 if ((c.Thumbprint == thumbprintCertificado))
    //                 {
    //                     cert = c;
    //                     break;
    //                 }
    //             }

    //             if ((cert == null))
    //             {
    //                 store = new X509Store("My", StoreLocation.LocalMachine);
    //                 CertCol = store.Certificates;
    //                 foreach (X509Certificate2 c in CertCol)
    //                 {
    //                     if ((c.Thumbprint == thumbprintCertificado))
    //                     {
    //                         cert = c;
    //                         break;
    //                     }
    //                 }
    //             }

    //             if ((cert == null))
    //             {
    //                 throw new CryptographicException("El certificado no se encuentra registrado");
    //             }
    //         }
    //         finally
    //         {
    //             store.Close();
    //         }
    //         return cert;
    //     }
    
}