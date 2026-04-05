using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retail.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4PromotionsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    coupon_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    max_uses = table.Column<int>(type: "integer", nullable: true),
                    max_uses_per_customer = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    can_stack_with_others = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotion_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_type = table.Column<int>(type: "integer", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    discount_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    target_reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    buy_quantity = table.Column<int>(type: "integer", nullable: true),
                    get_quantity = table.Column<int>(type: "integer", nullable: true),
                    buy_reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    get_reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_actions", x => x.id);
                    table.ForeignKey(
                        name: "FK_promotion_actions_promotions_promotion_id",
                        column: x => x.promotion_id,
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotion_conditions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condition_type = table.Column<int>(type: "integer", nullable: false),
                    decimal_value = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    int_value = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_conditions", x => x.id);
                    table.ForeignKey(
                        name: "FK_promotion_conditions_promotions_promotion_id",
                        column: x => x.promotion_id,
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotion_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_usages", x => x.id);
                    table.ForeignKey(
                        name: "FK_promotion_usages_promotions_promotion_id",
                        column: x => x.promotion_id,
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_actions_promotion_id",
                table: "promotion_actions",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_conditions_promotion_id",
                table: "promotion_conditions",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_usages_customer_id",
                table: "promotion_usages",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_usages_promotion_id",
                table: "promotion_usages",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotions_coupon_code_unique",
                table: "promotions",
                column: "coupon_code",
                unique: true,
                filter: "coupon_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_promotions_is_active_valid_from_valid_to",
                table: "promotions",
                columns: new[] { "is_active", "valid_from", "valid_to" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promotion_actions");

            migrationBuilder.DropTable(
                name: "promotion_conditions");

            migrationBuilder.DropTable(
                name: "promotion_usages");

            migrationBuilder.DropTable(
                name: "promotions");
        }
    }
}
