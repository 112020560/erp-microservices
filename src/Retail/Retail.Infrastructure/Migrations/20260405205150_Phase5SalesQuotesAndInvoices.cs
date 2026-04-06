using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retail.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase5SalesQuotesAndInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "number_sequences",
                columns: table => new
                {
                    sequence_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    current_value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_number_sequences", x => x.sequence_name);
                });

            migrationBuilder.CreateTable(
                name: "sale_invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cashier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requires_electronic_invoice = table.Column<bool>(type: "boolean", nullable: false),
                    electronic_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sale_quotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sales_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    volume_discount_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    promotion_discount_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_quotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_lines_sale_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "sale_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "applied_promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applied_promotions", x => x.id);
                    table.ForeignKey(
                        name: "FK_applied_promotions_sale_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "sale_quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_quote_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    price_list_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    resolution_source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_quote_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_sale_quote_lines_sale_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "sale_quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_applied_promotions_quote_id",
                table: "applied_promotions",
                column: "quote_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_lines_invoice_id",
                table: "payment_lines",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_quote_lines_quote_id",
                table: "sale_quote_lines",
                column: "quote_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_quotes_customer_id",
                table: "sale_quotes",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_quotes_sales_person_id",
                table: "sale_quotes",
                column: "sales_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_quotes_status",
                table: "sale_quotes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_sale_quotes_valid_until",
                table: "sale_quotes",
                column: "valid_until");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "applied_promotions");

            migrationBuilder.DropTable(
                name: "number_sequences");

            migrationBuilder.DropTable(
                name: "payment_lines");

            migrationBuilder.DropTable(
                name: "sale_quote_lines");

            migrationBuilder.DropTable(
                name: "sale_invoices");

            migrationBuilder.DropTable(
                name: "sale_quotes");
        }
    }
}
