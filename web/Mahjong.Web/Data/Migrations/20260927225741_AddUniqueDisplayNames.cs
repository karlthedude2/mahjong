using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mahjong.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDisplayNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayNameKey",
                table: "AspNetUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Existing players get their key. If two already share a name, the first account keeps it
            // for uniqueness and the other keeps its name without a key, so the index can be built.
            migrationBuilder.Sql("""
                UPDATE u SET DisplayNameKey = k.NameKey
                FROM AspNetUsers u
                JOIN (SELECT Id, UPPER(DisplayName) AS NameKey,
                             ROW_NUMBER() OVER (PARTITION BY UPPER(DisplayName) ORDER BY Id) AS Position
                      FROM AspNetUsers WHERE DisplayName <> N'') k ON k.Id = u.Id
                WHERE k.Position = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DisplayNameKey",
                table: "AspNetUsers",
                column: "DisplayNameKey",
                unique: true,
                filter: "[DisplayNameKey] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DisplayNameKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayNameKey",
                table: "AspNetUsers");
        }
    }
}
