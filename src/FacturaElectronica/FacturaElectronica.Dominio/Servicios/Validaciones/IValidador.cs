namespace FacturaElectronica.Dominio.Servicios.Validaciones;

// <summary>
/// Interfaz base para validadores
/// </summary>
public interface IValidador<T>
{
    ResultadoValidacion Validar(T entidad);
}