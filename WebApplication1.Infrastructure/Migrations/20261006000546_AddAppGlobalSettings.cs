using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppGlobalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppGlobalSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGlobalSettings", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppGlobalSettings");
        }
    }
}
