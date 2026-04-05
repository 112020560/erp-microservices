using FacturaElectronica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tenants
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantEmitterConfig> TenantEmitterConfigs => Set<TenantEmitterConfig>();
    public DbSet<TenantCertificateConfig> TenantCertificateConfigs => Set<TenantCertificateConfig>();
    public DbSet<TenantHaciendaConfig> TenantHaciendaConfigs => Set<TenantHaciendaConfig>();

    // Invoicing
    public DbSet<ElectronicInvoice> ElectronicDocuments => Set<ElectronicInvoice>();
    public DbSet<ElectronicDocumentLog> ElectronicDocumentLogs => Set<ElectronicDocumentLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Schema: tenants
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants", "tenants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            e.Property(x => x.TaxId).HasColumnName("tax_id").HasMaxLength(20).IsRequired();
            e.Property(x => x.TaxIdType).HasColumnName("tax_id_type").HasMaxLength(5).IsRequired();
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasOne(x => x.EmitterConfig).WithOne(x => x.Tenant).HasForeignKey<TenantEmitterConfig>(x => x.TenantId);
            e.HasOne(x => x.CertificateConfig).WithOne(x => x.Tenant).HasForeignKey<TenantCertificateConfig>(x => x.TenantId);
            e.HasOne(x => x.HaciendaConfig).WithOne(x => x.Tenant).HasForeignKey<TenantHaciendaConfig>(x => x.TenantId);
        });

        modelBuilder.Entity<TenantEmitterConfig>(e =>
        {
            e.ToTable("tenant_emitter_config", "tenants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(255).IsRequired();
            e.Property(x => x.NumeroIdentificacion).HasColumnName("numero_identificacion").HasMaxLength(20).IsRequired();
            e.Property(x => x.TipoIdentificacion).HasColumnName("tipo_identificacion").HasMaxLength(5).IsRequired();
            e.Property(x => x.Provincia).HasColumnName("provincia").HasMaxLength(3);
            e.Property(x => x.Canton).HasColumnName("canton").HasMaxLength(3);
            e.Property(x => x.Distrito).HasColumnName("distrito").HasMaxLength(3);
            e.Property(x => x.Barrio).HasColumnName("barrio").HasMaxLength(3);
            e.Property(x => x.OtrasSenas).HasColumnName("otras_senas").HasMaxLength(500);
            e.Property(x => x.CorreoElectronico).HasColumnName("correo_electronico").HasMaxLength(255);
            e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TenantCertificateConfig>(e =>
        {
            e.ToTable("tenant_certificate_config", "tenants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.CertificatePath).HasColumnName("certificate_path").HasMaxLength(500).IsRequired();
            e.Property(x => x.CertificateKeyEncrypted).HasColumnName("certificate_key_encrypted").HasMaxLength(1000).IsRequired();
            e.Property(x => x.ValidFrom).HasColumnName("valid_from");
            e.Property(x => x.ValidUntil).HasColumnName("valid_until");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TenantHaciendaConfig>(e =>
        {
            e.ToTable("tenant_hacienda_config", "tenants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.Environment).HasColumnName("environment").HasMaxLength(20).HasDefaultValue("sandbox");
            e.Property(x => x.ClientId).HasColumnName("client_id").HasMaxLength(255).IsRequired();
            e.Property(x => x.UsernameEncrypted).HasColumnName("username_encrypted").HasMaxLength(500).IsRequired();
            e.Property(x => x.PasswordEncrypted).HasColumnName("password_encrypted").HasMaxLength(1000).IsRequired();
            e.Property(x => x.AuthUrl).HasColumnName("auth_url").HasMaxLength(500).IsRequired();
            e.Property(x => x.SubmitUrl).HasColumnName("submit_url").HasMaxLength(500).IsRequired();
            e.Property(x => x.QueryUrl).HasColumnName("query_url").HasMaxLength(500).IsRequired();
            e.Property(x => x.MaxRetries).HasColumnName("max_retries").HasDefaultValue(3);
            e.Property(x => x.CallbackUrl).HasColumnName("callback_url").HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.TenantId).IsUnique();
        });

        // Schema: invoicing
        modelBuilder.Entity<ElectronicInvoice>(e =>
        {
            e.ToTable("electronic_documents", "invoicing");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
            e.Property(x => x.ExternalDocumentId).HasColumnName("external_document_id").HasMaxLength(100);
            e.Property(x => x.InvoiceType).HasColumnName("document_type").HasMaxLength(5).IsRequired();
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDIENTE");
            e.Property(x => x.StatusDetail).HasColumnName("status_detail");
            e.Property(x => x.Clave).HasColumnName("clave").HasMaxLength(50);
            e.Property(x => x.Consecutivo).HasColumnName("consecutivo").HasMaxLength(20);
            e.Property(x => x.EmisorIdentificacion).HasColumnName("emisor_identificacion").HasMaxLength(20);
            e.Property(x => x.ReceptorIdentificacion).HasColumnName("receptor_identificacion").HasMaxLength(20);
            e.Property(x => x.XmlEmisorPath).HasColumnName("xml_emisor_path").HasMaxLength(500);
            e.Property(x => x.XmlReceptorPath).HasColumnName("xml_receptor_path").HasMaxLength(500);
            e.Property(x => x.XmlRespuestaPath).HasColumnName("xml_respuesta_path").HasMaxLength(500);
            e.Property(x => x.FechaEmision).HasColumnName("fecha_emision");
            e.Property(x => x.FechaEnvio).HasColumnName("fecha_envio");
            e.Property(x => x.FechaRespuesta).HasColumnName("fecha_respuesta");
            e.Property(x => x.ResponseMessage).HasColumnName("response_message");
            e.Property(x => x.Error).HasColumnName("error");
            e.Property(x => x.ProcessType).HasColumnName("process_type").HasMaxLength(30).HasDefaultValue("polling");
            e.Property(x => x.RequiereCorreccion).HasColumnName("requires_correction").HasDefaultValue(false);
            e.Property(x => x.NotasCorreccion).HasColumnName("correction_notes");
            e.Property(x => x.FechaMarcadoCorreccion).HasColumnName("correction_marked_at");
            e.Property(x => x.CorreoReceptor).HasColumnName("correo_receptor").HasMaxLength(255);
            e.Property(x => x.TelefonoReceptor).HasColumnName("telefono_receptor").HasMaxLength(20);
            e.Property(x => x.NombreReceptor).HasColumnName("nombre_receptor").HasMaxLength(255);
            e.Property(x => x.NotificacionEnviada).HasColumnName("notificacion_enviada").HasDefaultValue(false);
            e.Property(x => x.FechaNotificacion).HasColumnName("fecha_notificacion");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Clave).IsUnique();
            e.HasIndex(x => new { x.TenantId, x.Status });
            e.HasMany(x => x.Logs).WithOne().HasForeignKey(x => x.DocumentId);
        });

        modelBuilder.Entity<ElectronicDocumentLog>(e =>
        {
            e.ToTable("electronic_document_logs", "invoicing");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.DocumentId).HasColumnName("document_id").IsRequired();
            e.Property(x => x.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            e.Property(x => x.Message).HasColumnName("message");
            e.Property(x => x.Details).HasColumnName("details");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });
    }
}
