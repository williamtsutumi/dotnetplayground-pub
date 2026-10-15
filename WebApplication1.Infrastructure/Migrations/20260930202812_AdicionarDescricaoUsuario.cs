using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDescricaoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Usuario",
                type: "character varying(1000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Usuario");
        }
    }
}
