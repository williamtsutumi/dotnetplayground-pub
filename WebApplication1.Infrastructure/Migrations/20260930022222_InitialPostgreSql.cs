using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeUsuario = table.Column<string>(type: "character varying(50)", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", nullable: false),
                    IdiomaPreferido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "EN"),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Senha = table.Column<string>(type: "character varying(255)", nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.UniqueConstraint("UQ_Usuario_Email", x => x.Email);
                    table.UniqueConstraint("UQ_Usuario_NomeUsuario", x => x.NomeUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Comunidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(50)", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", nullable: true),
                    IdCriadorUsuario = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunidade", x => x.Id);
                    table.UniqueConstraint("UQ_Comunidade_Nome", x => x.Nome);
                    table.ForeignKey(
                        name: "FK_Comunidade_Criador",
                        column: x => x.IdCriadorUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComunidadeUsuario",
                columns: table => new
                {
                    IdComunidade = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    Papel = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    EntrouEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunidadeUsuario", x => new { x.IdComunidade, x.IdUsuario });
                    table.ForeignKey(
                        name: "FK_ComunidadeUsuario_Comunidade",
                        column: x => x.IdComunidade,
                        principalTable: "Comunidade",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComunidadeUsuario_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Post",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComunidadeId = table.Column<int>(type: "integer", nullable: true),
                    IdAutorUsuario = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", nullable: false),
                    Conteudo = table.Column<string>(type: "text", nullable: false),
                    ConteudoMarkdown = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EditadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Post_Autor",
                        column: x => x.IdAutorUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_Comunidade",
                        column: x => x.ComunidadeId,
                        principalTable: "Comunidade",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Comentario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPost = table.Column<long>(type: "bigint", nullable: true),
                    IdComentario = table.Column<int>(type: "integer", nullable: true),
                    IdAutorUsuario = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentario_Autor",
                        column: x => x.IdAutorUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comentario_Comentario",
                        column: x => x.IdComentario,
                        principalTable: "Comentario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comentario_Post",
                        column: x => x.IdPost,
                        principalTable: "Post",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsuarioUpvotePost",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdPost = table.Column<long>(type: "bigint", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioUpvotePost", x => new { x.IdUsuario, x.IdPost });
                    table.ForeignKey(
                        name: "FK_UsuarioUpvotePost_Post",
                        column: x => x.IdPost,
                        principalTable: "Post",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UsuarioUpvotePost_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsuarioUpvoteComentario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdComentario = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioUpvoteComentario", x => new { x.IdUsuario, x.IdComentario });
                    table.ForeignKey(
                        name: "FK_UsuarioUpvoteComentario_Comentario",
                        column: x => x.IdComentario,
                        principalTable: "Comentario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UsuarioUpvoteComentario_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_IdAutorUsuario",
                table: "Comentario",
                column: "IdAutorUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_IdComentario",
                table: "Comentario",
                column: "IdComentario");

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_IdPost",
                table: "Comentario",
                column: "IdPost");

            migrationBuilder.CreateIndex(
                name: "IX_Comunidade_IdCriadorUsuario",
                table: "Comunidade",
                column: "IdCriadorUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_ComunidadeUsuario_IdUsuario",
                table: "ComunidadeUsuario",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ComunidadeId",
                table: "Post",
                column: "ComunidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_IdAutorUsuario",
                table: "Post",
                column: "IdAutorUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioUpvoteComentario_IdComentario",
                table: "UsuarioUpvoteComentario",
                column: "IdComentario");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioUpvotePost_IdPost",
                table: "UsuarioUpvotePost",
                column: "IdPost");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComunidadeUsuario");

            migrationBuilder.DropTable(
                name: "UsuarioUpvoteComentario");

            migrationBuilder.DropTable(
                name: "UsuarioUpvotePost");

            migrationBuilder.DropTable(
                name: "Comentario");

            migrationBuilder.DropTable(
                name: "Post");

            migrationBuilder.DropTable(
                name: "Comunidade");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
