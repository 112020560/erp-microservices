using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retail.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase3AdvancedVolumePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_volume_discounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_order_total = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    min_order_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    max_discount_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_volume_discounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_volume_discounts_price_lists_price_list_id",
                        column: x => x.price_list_id,
                        principalTable: "price_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_volume_discounts_list_active_priority",
                table: "order_volume_discounts",
                columns: new[] { "price_list_id", "is_active", "priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_volume_discounts");
        }
    }
}
