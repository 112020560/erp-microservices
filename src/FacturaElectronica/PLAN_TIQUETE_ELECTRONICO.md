# Plan: Implementación de Tiquete Electrónico (Tipo 04)

## Contexto
El sistema actualmente soporta Factura Electrónica (tipo 01). Se necesita agregar soporte para Tiquete Electrónico (tipo 04) que tiene un formato XML diferente.

## Diferencias entre Factura y Tiquete

| Aspecto | Factura Electrónica (01) | Tiquete Electrónico (04) |
|---------|--------------------------|--------------------------|
| **Elemento raíz** | `<FacturaElectronica>` | `<TiqueteElectronico>` |
| **Namespace** | `.../facturaElectronica` | `.../tiqueteElectronico` |
| **Receptor** | Obligatorio | Opcional (sin identificación) |
| **Uso típico** | B2B, crédito fiscal | B2C, consumidor final |

---

## Fases de Implementación

### FASE 1: Modelo XML para Tiquete Electrónico

**Archivo nuevo:** `FacturaElectronica.Dominio/Modelos/Xml/TiqueteElectronicoXml.cs`

```csharp
[XmlRoot("TiqueteElectronico", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico")]
public class TiqueteElectronicoXml
{
    // Misma estructura que FacturaElectronicaXml pero con namespace de tiquete
    // Receptor es opcional (puede ser null)
}
```

**Nota:** Las clases anidadas (EmisorXml, DetalleServicioXml, etc.) se pueden reutilizar.

---

### FASE 2: Generador de Documentos para Tiquete

**Archivo nuevo:** `FacturaElectronica.Dominio/Servicios/Factory/GeneradorTiqueteV44.cs`

- Implementa `IGeneradorDocumentos`
- Método principal: `CreaXMLTiqueteElectronico(Factura factura, string clave, string consecutivo)`
- Mapea el modelo de dominio `Factura` al modelo XML `TiqueteElectronicoXml`
- El receptor solo se incluye si tiene datos válidos

---

### FASE 3: Actualizar Factory de Generadores

**Archivo a modificar:** `FacturaElectronica.Dominio/Servicios/Factory/GeneradorDocumentosFactory.cs`

**Cambios:**
1. Agregar método `CrearGeneradorPorTipoDocumento(string tipoDocumento, VersionFacturaElectronica version)`
2. Si `tipoDocumento == "04"` → retornar `GeneradorTiqueteV44`
3. Si `tipoDocumento == "01"` → retornar `GeneradorDocumentosV44` (actual)

---

### FASE 4: Actualizar Interfaz IGeneradorDocumentos

**Archivo a modificar:** `FacturaElectronica.Dominio/Servicios/Factory/IGeneradorDocumentos.cs`

**Cambios:**
- Agregar método: `XmlDocument CreaXMLDocumentoElectronico(Factura factura, string clave, string consecutivo)`
- Este método genérico puede manejar tanto facturas como tiquetes

---

### FASE 5: Actualizar el Handler de Envío

**Archivo a modificar:** `FacturaElectronica.Aplicacion/ProcesoFactura/Enviar/EnviarFacturaCommandHandler.cs`

**Cambios en línea ~131:**
```csharp
// ANTES:
var generadorDocumentos = _generadorDocumentosFactory.CrearGenerador(VersionFacturaElectronica.V44);
var xmlFacturaSinFirmar = generadorDocumentos.CreaXMLFacturaElectronica(facturaModel, clave, numeroConsecutivo);

// DESPUÉS:
var generadorDocumentos = _generadorDocumentosFactory.CrearGeneradorPorTipoDocumento(
    facturaModel.TipoDocumento,
    VersionFacturaElectronica.V44);
var xmlDocumentoSinFirmar = generadorDocumentos.CreaXMLDocumentoElectronico(
    facturaModel,
    clave,
    numeroConsecutivo);
```

---

### FASE 6: Validaciones Específicas para Tiquete (Opcional)

**Archivo nuevo:** `FacturaElectronica.Dominio/Servicios/Validaciones/Tiquetes/ValidadorTiqueteV44.cs`

**Validaciones específicas:**
- Receptor opcional (no requerido para B2C)
- Posible límite de monto (según normativa Hacienda)
- Condición de venta típicamente "01" (Contado)

---

### FASE 7: Registro de Dependencias

**Archivo a modificar:** `FacturaElectronica.Dominio/Extensions/BusinessValidationsExtension.cs`

```csharp
services.AddScoped<GeneradorTiqueteV44>();
```

---

## Archivos a Crear (3)

| # | Archivo | Ubicación |
|---|---------|-----------|
| 1 | `TiqueteElectronicoXml.cs` | `Dominio/Modelos/Xml/` |
| 2 | `GeneradorTiqueteV44.cs` | `Dominio/Servicios/Factory/` |
| 3 | `ValidadorTiqueteV44.cs` (opcional) | `Dominio/Servicios/Validaciones/Tiquetes/` |

## Archivos a Modificar (4)

| # | Archivo | Cambios |
|---|---------|---------|
| 1 | `IGeneradorDocumentos.cs` | Agregar método genérico |
| 2 | `GeneradorDocumentosFactory.cs` | Agregar método por tipo documento |
| 3 | `EnviarFacturaCommandHandler.cs` | Usar factory con tipo documento |
| 4 | `BusinessValidationsExtension.cs` | Registrar nuevo generador |

---

## Flujo de Resolución

```
Request con TipoDocumento
         ↓
┌─────────────────────────────────┐
│ ¿TipoDocumento == "04"?        │
└─────────────────────────────────┘
    │                    │
  SÍ (Tiquete)       NO (Factura/Otros)
    ↓                    ↓
GeneradorTiqueteV44   GeneradorDocumentosV44
    ↓                    ↓
TiqueteElectronicoXml  FacturaElectronicaXml
    ↓                    ↓
    └────────┬───────────┘
             ↓
    XmlDocument firmado
             ↓
    Enviar a Hacienda
```

---

## Ejemplo de Uso (Request)

```json
{
  "documentoId": 123,
  "companiaId": 1,
  "tipoDocumento": "04",           // ← TIQUETE
  "consecutivoDocumento": 456,
  "fechaDocumento": "2024-01-15T10:30:00-06:00",
  "codigoActividad": "621100",
  "situacion": "01",

  "emisorNombre": "Mi Empresa S.A.",
  "emisorNumeroIdentificacion": "3101234567",
  "emisorTipoIdentificacion": "02",

  "receptor": false,               // ← Sin receptor para B2C

  "condicionVenta": "01",          // Contado
  "medioPago": "01",               // Efectivo

  "detalleServicios": [...],

  "totalVenta": 10000,
  "totalImpuesto": 1300,
  "totalComprobante": 11300,
  "codigoMoneda": "CRC"
}
```

---

## Verificación

1. Compilar proyecto `FacturaElectronica`
2. Enviar request con `tipoDocumento: "04"`
3. Verificar en logs que se genera XML con elemento `<TiqueteElectronico>`
4. Verificar respuesta exitosa de Hacienda
5. Enviar request con `tipoDocumento: "01"` para confirmar que facturas siguen funcionando

---

## Notas Importantes

1. **Namespace XSD v4.4 Tiquete:** `https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico`
2. **El proceso de firma es idéntico** - solo cambia el contenido XML
3. **El envío a Hacienda es idéntico** - mismo endpoint de recepción
4. **Reutilizar clases XML anidadas** - EmisorXml, DetalleServicioXml, etc. son compatibles
