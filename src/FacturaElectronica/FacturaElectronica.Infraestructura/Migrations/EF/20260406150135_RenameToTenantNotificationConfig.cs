using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturaElectronica.Infraestructura.Migrations.EF
{
    /// <inheritdoc />
    public partial class RenameToTenantNotificationConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_webhook_configs",
                schema: "tenants");

            migrationBuilder.CreateTable(
                name: "tenant_notification_configs",
                schema: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    webhook_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    webhook_secret = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    subscribed_events = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: "document.processed"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_notification_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_notification_configs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "tenants",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notification_configs_tenant_id",
                schema: "tenants",
                table: "tenant_notification_configs",
                column: "tenant_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_notification_configs",
                schema: "tenants");

            migrationBuilder.CreateTable(
                name: "tenant_webhook_configs",
                schema: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    subscribed_events = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: "document.processed"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    webhook_secret = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    webhook_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_webhook_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_webhook_configs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "tenants",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_webhook_configs_tenant_id",
                schema: "tenants",
                table: "tenant_webhook_configs",
                column: "tenant_id",
                unique: true);
        }
    }
}
