using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RetailOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataSprint2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "Category", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "TechBrand", "Electronics", "High-end smartphone", "Smartphone X1" },
                    { 2, "TechBrand", "Electronics", "Professional laptop", "Laptop Pro 15" },
                    { 3, "HomePlus", "Appliances", "Automatic coffee maker", "Coffee Maker" },
                    { 4, "PeripheralsInc", "Accessories", "Ergonomic mouse", "Wireless Mouse" },
                    { 5, "NatureLife", "Groceries", "Premium green tea", "Organic Green Tea" }
                });

            migrationBuilder.InsertData(
                table: "Stores",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "STR001", "Tienda Central" },
                    { 2, "STR002", "Sucursal Norte" }
                });

            migrationBuilder.InsertData(
                table: "Skus",
                columns: new[] { "Id", "AttributesJson", "Code", "Price", "ProductId" },
                values: new object[,]
                {
                    { 1, "{\"FABRICANTE\":\"TechFactory\", \"CONTENIDO\":\"Black Edition\"}", "SMX1-BLK", 799.99m, 1 },
                    { 2, "{\"FABRICANTE\":\"TechFactory\", \"CONTENIDO\":\"White Edition\"}", "SMX1-WHT", 799.99m, 1 },
                    { 3, "{\"FABRICANTE\":\"AssemblyCo\", \"CONTENIDO\":\"16GB RAM\"}", "LP15-16GB", 1299.00m, 2 },
                    { 4, "{\"FABRICANTE\":\"AssemblyCo\", \"CONTENIDO\":\"32GB RAM\"}", "LP15-32GB", 1599.00m, 2 },
                    { 5, "{\"FABRICANTE\":\"HomeBuild\", \"CONTENIDO\":\"Standard\"}", "CM-AUTO", 89.50m, 3 },
                    { 6, "{\"FABRICANTE\":\"LogiParts\", \"CONTENIDO\":\"Silent Clicks\"}", "WM-SILENT", 25.00m, 4 },
                    { 7, "{\"FABRICANTE\":\"EarthHarvest\", \"CONTENIDO\":\"50 Bags\"}", "GT-ORG-50", 12.00m, 5 }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "OnHand", "Reserved", "SkuId", "StoreId" },
                values: new object[,]
                {
                    { 1, 10, 0, 1, 1 },
                    { 2, 5, 0, 2, 1 },
                    { 3, 3, 0, 3, 1 },
                    { 4, 2, 0, 4, 1 },
                    { 5, 50, 0, 5, 1 },
                    { 6, 100, 0, 6, 1 },
                    { 7, 20, 0, 7, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Skus",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Skus",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Skus",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Skus",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Skus",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Skus",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Skus",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
