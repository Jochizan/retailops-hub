using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RetailOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeTypesAndTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Skus_SkuId",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Skus_Products_ProductId",
                table: "Skus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Skus",
                table: "Skus");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sku_AttributesJson_IsJson",
                table: "Skus");

            migrationBuilder.RenameTable(
                name: "Skus",
                newName: "skus");

            migrationBuilder.RenameIndex(
                name: "IX_Skus_ProductId",
                table: "skus",
                newName: "IX_skus_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Skus_Code",
                table: "skus",
                newName: "IX_skus_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_skus",
                table: "skus",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "attribute_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attribute_types", x => x.Id);
                    table.CheckConstraint("CK_attribute_types_scope", "Scope IN ('PRODUCT','SKUJSON')");
                });

            migrationBuilder.InsertData(
                table: "attribute_types",
                columns: new[] { "Id", "Code", "Name", "Scope" },
                values: new object[,]
                {
                    { 1, "FABRICANTE", "Fabricante", "SKUJSON" },
                    { 2, "MARCA", "Marca", "PRODUCT" },
                    { 3, "CONTENIDO", "Contenido", "SKUJSON" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_skus_attributes_isjson",
                table: "skus",
                sql: "ISJSON(AttributesJson) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_attribute_types_Code",
                table: "attribute_types",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_skus_SkuId",
                table: "Inventory",
                column: "SkuId",
                principalTable: "skus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_skus_Products_ProductId",
                table: "skus",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.TR_skus_validate_attributesjson
ON dbo.skus
AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;

  -- JSON válido
  IF EXISTS (SELECT 1 FROM inserted i WHERE ISJSON(i.AttributesJson) = 0)
  BEGIN
    RAISERROR('AttributesJson debe ser JSON valido.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END;

  -- Claves permitidas: solo las que están definidas como SKUJSON
  IF EXISTS (
    SELECT 1
    FROM inserted i
    CROSS APPLY OPENJSON(i.AttributesJson) j
    LEFT JOIN dbo.attribute_types t
      ON t.code COLLATE DATABASE_DEFAULT = j.[key] COLLATE DATABASE_DEFAULT AND t.scope = 'SKUJSON'
    WHERE t.id IS NULL
  )
  BEGIN
    RAISERROR('AttributesJson solo permite atributos definidos con scope SKUJSON (ej. FABRICANTE, CONTENIDO). MARCA vive en Product.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END;
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_skus_validate_attributesjson");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_skus_SkuId",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_skus_Products_ProductId",
                table: "skus");

            migrationBuilder.DropTable(
                name: "attribute_types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_skus",
                table: "skus");

            migrationBuilder.DropCheckConstraint(
                name: "CK_skus_attributes_isjson",
                table: "skus");

            migrationBuilder.RenameTable(
                name: "skus",
                newName: "Skus");

            migrationBuilder.RenameIndex(
                name: "IX_skus_ProductId",
                table: "Skus",
                newName: "IX_Skus_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_skus_Code",
                table: "Skus",
                newName: "IX_Skus_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Skus",
                table: "Skus",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sku_AttributesJson_IsJson",
                table: "Skus",
                sql: "ISJSON(AttributesJson) = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Skus_SkuId",
                table: "Inventory",
                column: "SkuId",
                principalTable: "Skus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Skus_Products_ProductId",
                table: "Skus",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
