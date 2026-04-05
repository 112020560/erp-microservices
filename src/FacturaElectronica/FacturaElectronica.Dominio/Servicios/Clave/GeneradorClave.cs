namespace FacturaElectronica.Dominio.Servicios.Clave;

public class GeneradorClave
{
    public string GenerateCreditNoteKey(string numeroConsecutivo, string cedulaJuridica, DateTime fechaEmision)//, string tipoDocumento)//, string sucursal, string situacion)
    {
        var fecha = fechaEmision;
        var fechaStr = fecha.ToString("ddMMyyyy"); // 8 dígitos

        var cedulaEmisor = cedulaJuridica.PadLeft(12, '0'); // 12 dígitos

        var consecutivo = numeroConsecutivo.ToString()
            .Replace("-", "")
            .Substring(10); // Últimos 10 dígitos del consecutivo
        consecutivo = consecutivo.PadLeft(12, '0'); // 12 dígitos

        var situacion = "1"; // 1=Normal

        var codigoSeguridad = GenerarCodigoSeguridad(); // 8 dígitos

        // Tipo de documento: 03 = Nota de Crédito
        var sucursal = numeroConsecutivo.Substring(0, 3);
        var terminal = numeroConsecutivo.Substring(3, 5);
        var tipo = "03"; // Nota de Crédito

        var claveSinDigito = $"{fechaStr}{cedulaEmisor}{consecutivo}{situacion}{codigoSeguridad}{tipo}{sucursal}{terminal}";

        // Calcular dígito verificador (algoritmo ATV)
        var digitoVerificador = CalcularDigitoVerificadorATV(claveSinDigito);

        return $"{claveSinDigito}{digitoVerificador}";
    }

    public string GenerateInvoiceKey(string numeroConsecutivoCompleto,
                                    string cedulaJuridica,
                                    DateTime fechaEmision,
                                    string situacion)
    {
        // /*
		// Posición: 1-3   4-6   7-14      15-26         27-29   30-40      41-50
		// Formato:  CPJ - CTE - FECHA  - CEDULA      - SIT - CONSEC  - CODIGO_SEG
		// Ejemplo:  506   14   03012025   101234567   001   0000012345   12345678
		// */

        // string pais = "506";


        // string dia = fechaEmision.Day.ToString("00");
        // string mes = fechaEmision.Month.ToString("00");
        // string año = fechaEmision.Year.ToString("0000");
        // string fecha = dia + mes + año;

        // string emisorId = cedulaJuridica.PadLeft(12, '0');
        // //string situacion = "001"; // 001 = Normal, 002 = Contingencia, 003 = Sin internet
        // string codigoSeguridad = GenerarCodigoSeguridad();

        // // Formato: TT SSSS NNNNNNN (Tipo + Sucursal + Consecutivo)
        // //string tipoDocumento = "01"; // 01=Factura, 02=Nota Débito, 03=Nota Crédito, 04=Tiquete, etc.
        // //string sucursal = "0001"; // Código de sucursal (4 dígitos)
        // //string consecutivo = "0000001"; // Número consecutivo (7 dígitos)
        // //string numeroConsecutivo = tipoDocumento + sucursal + consecutivo;


        // // 4. Situación (3 dígitos)
        // string sit = situacion;

        // // 5. Número consecutivo (11 dígitos)
        // // Formato: TipoDoc(2) + Sucursal(4) + Consecutivo(5)
        // string consec = tipoDocumento +
        //                sucursal.PadLeft(4, '0') +
        //                numeroConsecutivo.ToString().PadLeft(5, '0');

        // string clave = pais + fecha + emisorId + sit + consec + codigoSeguridad;

        // // Validar que sean exactamente 50 dígitos
        // if (clave.Length != 50)
        // {
        //     throw new InvalidOperationException($"La clave debe tener 50 dígitos. Actual: {clave.Length}");
        // }

        // return clave;
        // VALIDACIONES
            if (string.IsNullOrEmpty(numeroConsecutivoCompleto) || 
                numeroConsecutivoCompleto.Length != 20)
            {
                throw new ArgumentException(
                    "El consecutivo debe tener 20 dígitos. " +
                    "Formato: SSS-TTTTT-TT-NNNNNNNNNN (001-00001-01-0000000123)");
            }
            
            if (situacion != "1" && situacion != "2" && situacion != "3")
            {
                throw new ArgumentException("Situación debe ser 1, 2 o 3");
            }
            
            // ============================================
            // 1. FECHA (8 dígitos): ddmmaaaa
            // ============================================
            string dia = fechaEmision.Day.ToString("D2");
            string mes = fechaEmision.Month.ToString("D2");
            string año = fechaEmision.Year.ToString("D4");
            string fecha = dia + mes + año; // Ejemplo: 15012024
            
            // ============================================
            // 2. CÉDULA EMISOR (12 dígitos)
            // ============================================
            string cedulaLimpia = cedulaJuridica.Replace("-", "").Replace(" ", "");
            string cedula = cedulaLimpia.PadLeft(12, '0'); // Ejemplo: 003101234567
            
            // ============================================
            // 3. CONSECUTIVO (12 dígitos)
            // ============================================
            // Del consecutivo completo (20 dígitos): 00100001010000000123
            // Extraer los últimos 10 dígitos del número: 0000000123
            // Y rellenar a 12 dígitos: 000000000123
            
            // Descomponer el consecutivo:
            // Posiciones 0-2:   Sucursal (001)
            // Posiciones 3-7:   Terminal (00001)
            // Posiciones 8-9:   Tipo documento (01)
            // Posiciones 10-19: Número consecutivo (0000000123)
            
            string numeroSolo = numeroConsecutivoCompleto.Substring(10); // Últimos 10
            string consecutivo = numeroSolo.PadLeft(12, '0'); // Ejemplo: 000000000123
            
            // ============================================
            // 4. SITUACIÓN (1 dígito)
            // ============================================
            // Ya viene como parámetro: "1", "2" o "3"
            
            // ============================================
            // 5. CÓDIGO DE SEGURIDAD (8 dígitos aleatorios)
            // ============================================
            string codigoSeguridad = GenerarCodigoSeguridad(); // Ejemplo: 12345678
            
            // ============================================
            // 6. TIPO DE DOCUMENTO (2 dígitos)
            // ============================================
            // Extraer del consecutivo completo
            string tipoDocumento = numeroConsecutivoCompleto.Substring(8, 2); // Ejemplo: 01
            
            // ============================================
            // 7. SUCURSAL (3 dígitos)
            // ============================================
            string sucursal = numeroConsecutivoCompleto.Substring(0, 3); // Ejemplo: 001
            
            // ============================================
            // 8. TERMINAL (4 dígitos)
            // ============================================
            // Extraer 5 caracteres desde posición 3, pero tomar solo 4
            string terminalCompleto = numeroConsecutivoCompleto.Substring(3, 5); // 00001
            string terminal = terminalCompleto.Substring(0, 4).PadLeft(4, '0'); // 0000
            
            // ============================================
            // CONSTRUIR CLAVE SIN DÍGITO VERIFICADOR (49 dígitos)
            // ============================================
            string claveSinDigito = 
                fecha +              // 8 dígitos
                cedula +             // 12 dígitos
                consecutivo +        // 12 dígitos
                situacion +          // 1 dígito
                codigoSeguridad +    // 8 dígitos
                tipoDocumento +      // 2 dígitos
                sucursal +           // 3 dígitos
                terminal;            // 4 dígitos (en lugar de 3)
                                     // TOTAL: 49 dígitos
            
            // VALIDAR QUE SEAN 49 DÍGITOS
            if (claveSinDigito.Length != 49)
            {
                throw new InvalidOperationException(
                    $"Error interno: La clave sin dígito debe tener 49 dígitos. " +
                    $"Actual: {claveSinDigito.Length}");
            }
            
            // ============================================
            // 9. CALCULAR DÍGITO VERIFICADOR (Algoritmo ATV)
            // ============================================
            int digitoVerificador = CalcularDigitoVerificadorATV(claveSinDigito);
            
            // ============================================
            // CLAVE COMPLETA (50 dígitos)
            // ============================================
            string claveCompleta = claveSinDigito + digitoVerificador.ToString();
            
            // VALIDACIÓN FINAL
            if (claveCompleta.Length != 50)
            {
                throw new InvalidOperationException(
                    $"La clave debe tener 50 dígitos. Actual: {claveCompleta.Length}");
            }
            
            return claveCompleta;

    }

    private string GenerarCodigoSeguridad()
    {
        Random random = new Random();
        return random.Next(10000000, 99999999).ToString();
    }
    
    private int CalcularDigitoVerificadorATV(string clave)
        {
            // Algoritmo ATV (Verificador de 10)
            int suma = 0;
            int multiplicador = 2;
            
            for (int i = clave.Length - 1; i >= 0; i--)
            {
                int digito = int.Parse(clave[i].ToString());
                suma += digito * multiplicador;
                multiplicador = multiplicador == 2 ? 1 : 2;
            }
            
            int residuo = suma % 10;
            return residuo == 0 ? 0 : 10 - residuo;
        }
}