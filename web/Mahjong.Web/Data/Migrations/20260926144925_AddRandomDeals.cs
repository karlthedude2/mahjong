using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mahjong.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRandomDeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RandomDeal",
                table: "HighScores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RandomDeal",
                table: "Games",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PreferRandomDeals",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RandomDeal",
                table: "HighScores");

            migrationBuilder.DropColumn(
                name: "RandomDeal",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PreferRandomDeals",
                table: "AspNetUsers");
        }
    }
}
