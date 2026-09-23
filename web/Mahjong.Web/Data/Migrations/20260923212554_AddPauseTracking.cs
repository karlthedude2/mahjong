using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mahjong.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPauseTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PausedAtUtc",
                table: "Games",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PausedSeconds",
                table: "Games",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PausedAtUtc",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PausedSeconds",
                table: "Games");
        }
    }
}
