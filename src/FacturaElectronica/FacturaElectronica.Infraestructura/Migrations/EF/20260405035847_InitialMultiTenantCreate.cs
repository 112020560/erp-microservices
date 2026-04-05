using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturaElectronica.Infraestructura.Migrations.EF
{
    /// <inheritdoc />
    public partial class InitialMultiTenantCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "invoicing");

            migrationBuilder.EnsureSchema(
                name: "tenants");

            migrationBuilder.CreateTable(
                name: "electronic_documents",
                schema: "invoicing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_document_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    document_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDIENTE"),
                    status_detail = table.Column<string>(type: "text", nullable: true),
                    clave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consecutivo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    emisor_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    receptor_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    xml_emisor_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    xml_receptor_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    xml_respuesta_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_emision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_respuesta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    response_message = table.Column<string>(type: "text", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    process_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, defaultValue: "polling"),
                    requires_correction = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    correction_notes = table.Column<string>(type: "text", nullable: true),
                    correction_marked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    correo_receptor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    telefono_receptor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    nombre_receptor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    notificacion_enviada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_notificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_electronic_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tax_id_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "electronic_document_logs",
                schema: "invoicing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    details = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_electronic_document_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_electronic_document_logs_electronic_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "invoicing",
                        principalTable: "electronic_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_certificate_config",
                schema: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    certificate_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    certificate_key_encrypted = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_certificate_config", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_certificate_config_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "tenants",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_emitter_config",
                schema: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    numero_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tipo_identificacion = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    provincia = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    canton = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    distrito = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    barrio = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    otras_senas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    correo_electronico = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_emitter_config", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_emitter_config_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "tenants",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_hacienda_config",
                schema: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "sandbox"),
                    client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    username_encrypted = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    password_encrypted = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    auth_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    submit_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    query_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    max_retries = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    callback_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_hacienda_config", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_hacienda_config_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "tenants",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_logs_document_id",
                schema: "invoicing",
                table: "electronic_document_logs",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_documents_clave",
                schema: "invoicing",
                table: "electronic_documents",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_electronic_documents_tenant_id_status",
                schema: "invoicing",
                table: "electronic_documents",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_certificate_config_tenant_id",
                schema: "tenants",
                table: "tenant_certificate_config",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_emitter_config_tenant_id",
                schema: "tenants",
                table: "tenant_emitter_config",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_hacienda_config_tenant_id",
                schema: "tenants",
                table: "tenant_hacienda_config",
                column: "tenant_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "electronic_document_logs",
                schema: "invoicing");

            migrationBuilder.DropTable(
                name: "tenant_certificate_config",
                schema: "tenants");

            migrationBuilder.DropTable(
                name: "tenant_emitter_config",
                schema: "tenants");

            migrationBuilder.DropTable(
                name: "tenant_hacienda_config",
                schema: "tenants");

            migrationBuilder.DropTable(
                name: "electronic_documents",
                schema: "invoicing");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "tenants");
        }
    }
}
