using FacturaElectronica.Dominio.Modelos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;

/// <summary>
/// Implementación del validador de códigos con caché en memoria
/// </summary>
public class ValidadorCodigosHacienda : IValidadorCodigosHacienda
{
    private readonly ILogger<ValidadorCodigosHacienda> _logger;
    private readonly IMemoryCache _cache;

    // Códigos válidos según tablas de Hacienda v4.4
    private readonly Dictionary<string, string[]> _codigosValidos;

    public ValidadorCodigosHacienda(ILogger<ValidadorCodigosHacienda> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
        _codigosValidos = InicializarCodigosValidos();
    }

    public ResultadoValidacion ValidarFactura(Factura factura)
    {
        var resultado = new ResultadoValidacion();

        // Validar tipo de documento
        if (!string.IsNullOrEmpty(factura.TipoDocumento))
        {
            var validacion = ValidarTipoDocumento(factura.TipoDocumento);
            resultado.Errores.AddRange(validacion.Errores);
        }

        // Validar condición de venta
        if (!string.IsNullOrEmpty(factura.CondicionVenta))
        {
            var validacion = ValidarCondicionVenta(factura.CondicionVenta);
            resultado.Errores.AddRange(validacion.Errores);
        }

        // Validar medio de pago
        if (!string.IsNullOrEmpty(factura.MedioPago))
        {
            var validacion = ValidarMedioPago(factura.MedioPago);
            resultado.Errores.AddRange(validacion.Errores);
        }

        // Validar código de actividad económica (CIIU 4) - Obligatorio en v4.4
        if (!string.IsNullOrEmpty(factura.CodigoActividad))
        {
            var validacion = ValidarCodigoActividad(factura.CodigoActividad);
            resultado.Errores.AddRange(validacion.Errores);
        }

        // Validar moneda si es diferente de CRC
        if (!string.IsNullOrEmpty(factura.CodigoMoneda) && factura.CodigoMoneda != "CRC")
        {
            var validacion = ValidarCodigoMoneda(factura.CodigoMoneda);
            resultado.Errores.AddRange(validacion.Errores);
        }

        return resultado;
    }

    public ResultadoValidacion ValidarTipoIdentificacion(string tipoIdentificacion)
    {
        return ValidarCodigo("TiposIdentificacion", tipoIdentificacion, "tipo de identificación");
    }

    public ResultadoValidacion ValidarCodigoActividad(string codigoActividad)
    {
        var resultado = new ResultadoValidacion();

        if (string.IsNullOrWhiteSpace(codigoActividad))
        {
            resultado.AgregarError("El código de actividad económica es obligatorio en v4.4");
            return resultado;
        }

        // Validar formato CIIU 4: acepta 6 dígitos o formato con punto (ej: "620100", "6201.0", "6201.00")
        if (!System.Text.RegularExpressions.Regex.IsMatch(codigoActividad, @"^\d{4,6}(\.\d{1,2})?$"))
        {
            resultado.AgregarError($"El código de actividad tiene formato inválido: {codigoActividad}");
            return resultado;
        }

        return resultado;
    }

    public ResultadoValidacion ValidarCondicionVenta(string condicionVenta)
    {
        return ValidarCodigo("CondicionesVenta", condicionVenta, "condición de venta");
    }

    public ResultadoValidacion ValidarMedioPago(string medioPago)
    {
        return ValidarCodigo("MediosPago", medioPago, "medio de pago");
    }

    public ResultadoValidacion ValidarUnidadMedida(string unidadMedida)
    {
        return ValidarCodigo("UnidadesMedida", unidadMedida, "unidad de medida");
    }

    public ResultadoValidacion ValidarCodigoCABYS(string codigoCABYS)
    {
        var resultado = new ResultadoValidacion();

        if (string.IsNullOrWhiteSpace(codigoCABYS))
            return resultado; // CABYS es opcional

        // Validar formato CABYS (13 dígitos para v2025)
        if (!System.Text.RegularExpressions.Regex.IsMatch(codigoCABYS, @"^\d{13}$"))
        {
            resultado.AgregarAdvertencia($"El código CABYS debe tener 13 dígitos: {codigoCABYS}");
            return resultado;
        }

        // Validación adicional: verificar contra catálogo CABYS 2025
        // Esto requeriría acceso a la tabla completa de CABYS
        if (!ValidarExistenciaCABYS(codigoCABYS))
        {
            resultado.AgregarAdvertencia($"El código CABYS podría no existir en el catálogo 2025: {codigoCABYS}");
        }

        return resultado;
    }

    public ResultadoValidacion ValidarCodigoImpuesto(string codigoImpuesto)
    {
        return ValidarCodigo("CodigosImpuesto", codigoImpuesto, "código de impuesto");
    }

    public ResultadoValidacion ValidarCodigoMoneda(string codigoMoneda)
    {
        return ValidarCodigo("CodigosMoneda", codigoMoneda, "código de moneda");
    }

    public ResultadoValidacion ValidarCodigoPais(string codigoPais)
    {
        return ValidarCodigo("CodigosPais", codigoPais, "código de país");
    }

    private ResultadoValidacion ValidarTipoDocumento(string tipoDocumento)
    {
        return ValidarCodigo("TiposDocumento", tipoDocumento, "tipo de documento");
    }

    private ResultadoValidacion ValidarCodigo(string categoria, string codigo, string descripcion)
    {
        var resultado = new ResultadoValidacion();

        if (string.IsNullOrWhiteSpace(codigo))
            return resultado;

        if (!_codigosValidos.TryGetValue(categoria, out var codigosValidos))
        {
            _logger.LogWarning("Categoría de códigos no encontrada: {Categoria}", categoria);
            resultado.AgregarAdvertencia($"No se pudo validar {descripcion}: categoría {categoria} no configurada");
            return resultado;
        }

        if (!codigosValidos.Contains(codigo))
        {
            resultado.AgregarError($"Código de {descripcion} inválido: {codigo}. Códigos válidos: {string.Join(", ", codigosValidos)}");
        }

        return resultado;
    }

    private bool ValidarExistenciaCABYS(string codigoCABYS)
    {
        // Implementar validación contra catálogo CABYS 2025
        // Esto podría ser una consulta a base de datos o servicio externo
        var cacheKey = $"CABYS_{codigoCABYS}";
        
        if (_cache.TryGetValue(cacheKey, out bool existe))
        {
            return existe;
        }

        // Aquí iría la lógica real de validación
        // Por ahora simulamos que existe si cumple el formato
        existe = System.Text.RegularExpressions.Regex.IsMatch(codigoCABYS, @"^\d{13}$");
        
        _cache.Set(cacheKey, existe, TimeSpan.FromHours(24));
        return existe;
    }

    private Dictionary<string, string[]> InicializarCodigosValidos()
    {
        return new Dictionary<string, string[]>
        {
            ["TiposDocumento"] = new[]
            {
                "01", // Factura Electrónica
                "02", // Nota de Débito
                "03", // Nota de Crédito
                "04", // Tiquete Electrónico
                "05", // Nota de Despacho
                "06", // Contrato
                "07", // Procedimiento
                "08", // Comprobante emitido en contingencia
                "09"  // Factura de Compra (v4.4)
            },

            ["TiposIdentificacion"] = new[]
            {
                "01", // Cédula Física
                "02", // Cédula Jurídica
                "03", // DIMEX
                "04", // NITE
                "05"  // Pasaporte (v4.4)
            },

            ["CondicionesVenta"] = new[]
            {
                "01", // Contado
                "02", // Crédito
                "03", // Consignación
                "04", // Apartado
                "05", // Arrendamiento con opción de compra
                "06", // Arrendamiento en función financiera
                "07", // Cobro favor tercero
                "08", // Servicios prestados Estado a crédito
                "09", // Pago Estado contratista
                "99"  // Otros
            },

            ["MediosPago"] = new[]
            {
                "01", // Efectivo
                "02", // Tarjeta
                "03", // Cheque
                "04", // Transferencia - Depósito bancario
                "05", // Recaudado por terceros
                "06", // Dinero electrónico Banco Central
                "07", // Sistemas de pago móvil
                "08", // Medios de pago digital Estado
                "09", // Servicios Estado
                "99"  // Otros
            },

            ["CodigosImpuesto"] = new[]
            {
                "01", // Impuesto al Valor Agregado
                "02", // Selectivo de Consumo
                "03", // Único a los Combustibles
                "04", // Específico de Bebidas Alcoholicas
                "05", // Bebidas envasadas sin contenido alcohólico
                "06", // Específico del Cemento
                "07", // Específico sobre las bolsas plásticas
                "08", // Impuesto al Valor Agregado (Devuelto)
                "99"  // Otros
            },

            ["CodigosMoneda"] = new[]
            {
                "CRC", // Colón costarricense
                "USD", // Dólar estadounidense
                "EUR", // Euro
                "GBP", // Libra esterlina
                "JPY", // Yen japonés
                "CAD", // Dólar canadiense
                "CHF", // Franco suizo
                "CNY"  // Yuan chino
            },

            ["CodigosPais"] = new[]
            {
                "506", // Costa Rica
                "001", // Estados Unidos
                "052", // México
                "057", // Colombia
                "484", // Guatemala
                "840", // Estados Unidos (ISO)
                "124", // Canadá
                "276", // Alemania
                "250", // Francia
                "724", // España
                "380", // Italia
                "826", // Reino Unido
                "392", // Japón
                "156"  // China
            },

            ["UnidadesMedida"] = new[]
            {
                "Unid", // Unidad (genérico) ✅ VÁLIDO
                "Sp",   // Servicios Profesionales
                "Mt",   // Metro
                "Kg",   // Kilogramo
                "s",    // Segundo
                "A",    // Amperio
                "K",    // Kelvin
                "mol",  // Mol
                "cd",   // Candela
                "m²",   // Metro cuadrado
                "m³",   // Metro cuadrado
                "l",    // Litro
                "Wh",   // Vatio hora
                "g",    // Gramo
                "°",    // Grado
                "1",    // Unidad
                "Al",   // Alquiler
                "Cm",   // Centímetro
                "dz",   // Docena
                "gal",  // Galón
                "h",    // Hora
                "kg",   // Kilogramo
                "km",   // Kilómetro
                "kW⋅h", // Kilovatio hora
                "lb",   // Libra
                "m",    // Metro
                "mm",   // Milímetro
                "oz",   // Onza
                "pkg",  // Paquete
                "t",    // Tonelada
                "un",   // Unidad
                "yd"    // Yarda
            }
        };
    }
}


/*
 [
     { "codigo": "01", "descripcion": "Descuento por volumen" },
     { "codigo": "02", "descripcion": "Descuento por pronto pago" },
     { "codigo": "03", "descripcion": "Descuento comercial" },
     { "codigo": "04", "descripcion": "Descuento por promoción" },
     { "codigo": "05", "descripcion": "Descuento por temporada" },
     { "codigo": "06", "descripcion": "Bonificación" },
     { "codigo": "07", "descripcion": "Rebaja" },
     { "codigo": "08", "descripcion": "Descuento por fidelidad" },
     { "codigo": "09", "descripcion": "Otros descuentos autorizados" }
     
     
     ┌────────┬──────────────────────────────────────────────┐                                                                                                                                                                                
   │ Código │                 Descripción                  │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 01     │ Descuento por Regalía (100% del monto)       │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 02     │ Descuento por Regalía IVA Cobrado al Cliente │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 03     │ Descuento por Bonificación                   │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 04     │ Descuento por volumen                        │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 05     │ Descuento por Temporada (estacional)         │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 06     │ Descuento promocional                        │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 07     │ Descuento Comercial ← Este es el correcto    │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 08     │ Descuento por frecuencia                     │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 09     │ Descuento sostenido                          │                                                                                                                                                                                
   ├────────┼──────────────────────────────────────────────┤                                                                                                                                                                                
   │ 99     │ Otros                                        │                                                                                                                                                                                
   └────────┴──────────────────────────────────────────────┘    
   ]
 */