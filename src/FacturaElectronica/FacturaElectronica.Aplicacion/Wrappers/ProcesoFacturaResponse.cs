// using System.Text.Json;

// namespace FacturaElectronica.Aplicacion.Wrappers;

// /// <summary>
// /// ═══════════════════════════════════════════════════════════════════════════════
// /// MODELO: ResultadoFacturaElectronica
// /// ═══════════════════════════════════════════════════════════════════════════════
// /// Ubicación: FacturaElectronica.Aplicacion/Wrappers/ResultadoFacturaElectronica.cs
// /// 
// /// Este es el objeto de respuesta que devuelve tu Handler
// /// </summary>
// public class ResultadoFacturaElectronica
// {
//     /// <summary>
//     /// Indica si la operación fue exitosa
//     /// </summary>
//     public bool Exitoso { get; set; }

//     /// <summary>
//     /// Mensaje principal de la respuesta
//     /// </summary>
//     public string Mensaje { get; set; } = string.Empty;

//     /// <summary>
//     /// Lista de errores (si los hay)
//     /// </summary>
//     public List<string> Errores { get; set; } = new();

//     /// <summary>
//     /// Lista de advertencias (no bloquean el proceso)
//     /// </summary>
//     public List<string> Advertencias { get; set; } = new();

//     /// <summary>
//     /// Datos adicionales de la respuesta (clave, consecutivo, etc.)
//     /// </summary>
//     public object? Data { get; set; }

//     /// <summary>
//     /// Timestamp de la operación
//     /// </summary>
//     public DateTime Timestamp { get; set; } = DateTime.UtcNow;

//     /// <summary>
//     /// Código de estado personalizado (opcional)
//     /// </summary>
//     public string? CodigoEstado { get; set; }

//     public override string ToString()
//     {
//         return JsonSerializer.Serialize(this);
//     }
// }