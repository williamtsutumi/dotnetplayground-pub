using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioBookmarkPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioBookmarkPost",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdPost = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioBookmarkPost", x => new { x.IdUsuario, x.IdPost });
                    table.ForeignKey(
                        name: "FK_UsuarioBookmarkPost_Post",
                        column: x => x.IdPost,
                        principalTable: "Post",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UsuarioBookmarkPost_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioBookmarkPost_IdPost",
                table: "UsuarioBookmarkPost",
                column: "IdPost");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioBookmarkPost");
        }
    }
}
