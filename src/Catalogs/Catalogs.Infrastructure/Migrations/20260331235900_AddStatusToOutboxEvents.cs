using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToOutboxEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_events_ProcessedOn_RetryCount_OccurredOn",
                table: "outbox_events");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "outbox_events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_events_Status_RetryCount_OccurredOn",
                table: "outbox_events",
                columns: new[] { "Status", "RetryCount", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_events_Status_RetryCount_OccurredOn",
                table: "outbox_events");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "outbox_events");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_events_ProcessedOn_RetryCount_OccurredOn",
                table: "outbox_events",
                columns: new[] { "ProcessedOn", "RetryCount", "OccurredOn" });
        }
    }
}
