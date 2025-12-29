using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReorderPointToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReorderPoint",
                table: "Inventory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "ReorderPoint",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "ReorderPoint",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "ReorderPoint",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "ReorderPoint",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "ReorderPoint",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 6,
                column: "ReorderPoint",
                value: 20);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 7,
                column: "ReorderPoint",
                value: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReorderPoint",
                table: "Inventory");
        }
    }
}
