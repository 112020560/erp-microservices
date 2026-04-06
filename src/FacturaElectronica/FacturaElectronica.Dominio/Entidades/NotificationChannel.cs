namespace FacturaElectronica.Dominio.Entidades;

[Flags]
public enum NotificationChannel
{
    None     = 0,
    RabbitMq = 1,
    Webhook  = 2
}
