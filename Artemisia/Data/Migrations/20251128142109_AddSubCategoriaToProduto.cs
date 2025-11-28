using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Artemisia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoriaToProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Categorias_ParentCategoriaId",
                table: "Categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos");

            migrationBuilder.AddColumn<int>(
                name: "SubCategoriaId",
                table: "Produtos",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_SubCategoriaId",
                table: "Produtos",
                column: "SubCategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_ParentCategoriaId",
                table: "Categorias",
                column: "ParentCategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Categorias_SubCategoriaId",
                table: "Produtos",
                column: "SubCategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Categorias_ParentCategoriaId",
                table: "Categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Categorias_SubCategoriaId",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_Produtos_SubCategoriaId",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "SubCategoriaId",
                table: "Produtos");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_ParentCategoriaId",
                table: "Categorias",
                column: "ParentCategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
