namespace FacturaElectronica.Dominio.Exceptions;

public class HaciendaAuthException : Exception
{
    public string? ErrorCode { get; }
    public string? ErrorDescription { get; }
    
    public HaciendaAuthException(string message) : base(message) { }
    
    public HaciendaAuthException(string errorCode, string errorDescription) 
        : base($"{errorCode}: {errorDescription}")
    {
        ErrorCode = errorCode;
        ErrorDescription = errorDescription;
    }
}