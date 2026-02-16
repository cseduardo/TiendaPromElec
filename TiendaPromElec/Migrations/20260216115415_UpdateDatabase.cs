using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaPromElec.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Password", "Role" },
                values: new object[] { "", "Client" });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Password", "Role" },
                values: new object[] { "", "Client" });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "Email", "FullName", "Password", "Phone", "Role" },
                values: new object[] { 3L, "Admin Address 1", "admin_one@promelec.com", "Admin One", "$2a$11$pZ5LcLatFwTF2JrfxBL9GO1qCmdr4dkl9tp.IL6KTYje.LtF794KO", "5550001111", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Customers");
        }
    }
}
