using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retail.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2CustomerGroupsAndScheduledChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_from",
                table: "channel_price_lists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_to",
                table: "channel_price_lists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "customer_group_price_lists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_group_price_lists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_price_changes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    new_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    new_discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    new_min_price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    effective_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    applied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_price_changes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer_group_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_group_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_group_members_customer_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "customer_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_group_members_group_customer",
                table: "customer_group_members",
                columns: new[] { "group_id", "customer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_group_price_lists_group_id",
                table: "customer_group_price_lists",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_group_price_lists_price_list_id",
                table: "customer_group_price_lists",
                column: "price_list_id");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_price_changes_price_list_id",
                table: "scheduled_price_changes",
                column: "price_list_id");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_price_changes_status_effective_at",
                table: "scheduled_price_changes",
                columns: new[] { "status", "effective_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_group_members");

            migrationBuilder.DropTable(
                name: "customer_group_price_lists");

            migrationBuilder.DropTable(
                name: "scheduled_price_changes");

            migrationBuilder.DropTable(
                name: "customer_groups");

            migrationBuilder.DropColumn(
                name: "valid_from",
                table: "channel_price_lists");

            migrationBuilder.DropColumn(
                name: "valid_to",
                table: "channel_price_lists");
        }
    }
}
