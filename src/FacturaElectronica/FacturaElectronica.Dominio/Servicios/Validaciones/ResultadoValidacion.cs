namespace FacturaElectronica.Dominio.Servicios.Validaciones;

public class ResultadoValidacion
{
    public bool EsValido => !Errores.Any();
    public List<string> Errores { get; } = new();
    public List<string> Advertencias { get; } = new();

    public void AgregarError(string error) => Errores.Add(error);
    public void AgregarAdvertencia(string advertencia) => Advertencias.Add(advertencia);

    public static ResultadoValidacion Exitoso() => new();
    public static ResultadoValidacion ConError(string error)
    {
        var resultado = new ResultadoValidacion();
        resultado.AgregarError(error);
        return resultado;
    }
}