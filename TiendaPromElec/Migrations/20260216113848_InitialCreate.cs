using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TiendaPromElec.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "Smartphones" },
                    { 2L, "Laptops" },
                    { 3L, "Accesorios" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "Email", "FullName", "Phone" },
                values: new object[,]
                {
                    { 1L, "Calle Falsa 123, CDMX", "juan.perez@email.com", "Juan Pérez", "5512345678" },
                    { 2L, "Av. Siempre Viva 742, Monterrey", "ana.garcia@email.com", "Ana García", "8187654321" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CustomerId", "OrderDate", "Status", "TotalAmount" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Entregado", 28999.50m },
                    { 2L, 2L, new DateTime(2025, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Enviado", 43700.75m },
                    { 3L, 1L, new DateTime(2025, 6, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Enviado", 5500.00m },
                    { 4L, 2L, new DateTime(2025, 6, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pendiente", 45999.98m },
                    { 5L, 1L, new DateTime(2025, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pendiente", 2400.00m }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1L, "Samsung", 1L, "El último smartphone de Samsung", "https://via.placeholder.com/300", "Galaxy S25", 24999.99m, 50 },
                    { 2L, "Apple", 1L, "El flagship de Apple", "https://via.placeholder.com/300", "iPhone 16 Pro", 28999.50m, 40 },
                    { 3L, "Google", 1L, "La experiencia pura de Android", "https://via.placeholder.com/300", "Pixel 9", 21500.00m, 30 },
                    { 4L, "Apple", 2L, "Potencia y portabilidad", "https://via.placeholder.com/300", "MacBook Air M4", 32500.00m, 25 },
                    { 5L, "Dell", 2L, "La mejor laptop para creadores", "https://via.placeholder.com/300", "Dell XPS 15", 38000.75m, 20 },
                    { 6L, "Lenovo", 2L, "El estándar para negocios", "https://via.placeholder.com/300", "ThinkPad X1 Carbon", 41200.00m, 15 },
                    { 7L, "Apple", 3L, "Cancelación de ruido superior", "https://via.placeholder.com/300", "AirPods Pro 3", 5500.00m, 100 },
                    { 8L, "Samsung", 3L, "Audio de alta fidelidad", "https://via.placeholder.com/300", "Samsung Galaxy Buds 4", 3800.00m, 120 },
                    { 9L, "Anker", 3L, "Carga rápida para todos tus dispositivos", "https://via.placeholder.com/300", "Cargador Anker 100W", 1200.00m, 200 },
                    { 10L, "Logitech", 3L, "El mouse definitivo para productividad", "https://via.placeholder.com/300", "Mouse Logitech MX Master 4", 2400.00m, 80 }
                });

            migrationBuilder.InsertData(
                table: "OrderDetails",
                columns: new[] { "Id", "OrderId", "ProductId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1L, 1L, 2L, 1, 28999.50m },
                    { 2L, 2L, 5L, 1, 38000.75m },
                    { 3L, 2L, 7L, 1, 5700.00m },
                    { 4L, 3L, 7L, 1, 5500.00m },
                    { 5L, 4L, 1L, 2, 22999.99m },
                    { 6L, 5L, 10L, 1, 2400.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
