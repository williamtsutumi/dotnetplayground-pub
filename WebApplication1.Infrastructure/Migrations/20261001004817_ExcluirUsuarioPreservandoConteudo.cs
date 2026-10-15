using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExcluirUsuarioPreservandoConteudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_Autor",
                table: "Comentario");

            migrationBuilder.DropForeignKey(
                name: "FK_Comunidade_Criador",
                table: "Comunidade");

            migrationBuilder.DropForeignKey(
                name: "FK_ComunidadeUsuario_Usuario",
                table: "ComunidadeUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Autor",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioUpvoteComentario_Usuario",
                table: "UsuarioUpvoteComentario");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioUpvotePost_Usuario",
                table: "UsuarioUpvotePost");

            migrationBuilder.AlterColumn<int>(
                name: "IdAutorUsuario",
                table: "Post",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "IdCriadorUsuario",
                table: "Comunidade",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "IdAutorUsuario",
                table: "Comentario",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_Autor",
                table: "Comentario",
                column: "IdAutorUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Comunidade_Criador",
                table: "Comunidade",
                column: "IdCriadorUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ComunidadeUsuario_Usuario",
                table: "ComunidadeUsuario",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Autor",
                table: "Post",
                column: "IdAutorUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioUpvoteComentario_Usuario",
                table: "UsuarioUpvoteComentario",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioUpvotePost_Usuario",
                table: "UsuarioUpvotePost",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_Autor",
                table: "Comentario");

            migrationBuilder.DropForeignKey(
                name: "FK_Comunidade_Criador",
                table: "Comunidade");

            migrationBuilder.DropForeignKey(
                name: "FK_ComunidadeUsuario_Usuario",
                table: "ComunidadeUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Autor",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioUpvoteComentario_Usuario",
                table: "UsuarioUpvoteComentario");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioUpvotePost_Usuario",
                table: "UsuarioUpvotePost");

            migrationBuilder.AlterColumn<int>(
                name: "IdAutorUsuario",
                table: "Post",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdCriadorUsuario",
                table: "Comunidade",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdAutorUsuario",
                table: "Comentario",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_Autor",
                table: "Comentario",
                column: "IdAutorUsuario",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comunidade_Criador",
                table: "Comunidade",
                column: "IdCriadorUsuario",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComunidadeUsuario_Usuario",
                table: "ComunidadeUsuario",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Autor",
                table: "Post",
                column: "IdAutorUsuario",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioUpvoteComentario_Usuario",
                table: "UsuarioUpvoteComentario",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioUpvotePost_Usuario",
                table: "UsuarioUpvotePost",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id");
        }
    }
}
