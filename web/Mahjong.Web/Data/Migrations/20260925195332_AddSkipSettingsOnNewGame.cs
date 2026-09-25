using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mahjong.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSkipSettingsOnNewGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SkipSettingsOnNewGame",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkipSettingsOnNewGame",
                table: "AspNetUsers");
        }
    }
}
