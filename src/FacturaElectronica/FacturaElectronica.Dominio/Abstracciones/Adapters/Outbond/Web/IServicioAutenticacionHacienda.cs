using FacturaElectronica.Dominio.Modelos.Fiscal;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;

public interface IServicioAutenticacionHacienda
{
    /// <summary>
    /// Obtener token de acceso (con caché automático)
    /// </summary>
    Task<string> ObtenerTokenAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Forzar obtención de nuevo token
    /// </summary>
    Task<ResultadoToken> ObtenerNuevoTokenAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidar token en caché
    /// </summary>
    void InvalidarToken();
}