using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Artemisia.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateProdutoCategoria_20251126 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Produtos",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Categorias",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Produtos",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Categorias",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
