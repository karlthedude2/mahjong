using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mahjong.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReplays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReplayOfGameId",
                table: "Games",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Seconds",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReplayScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalGameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Seconds = table.Column<int>(type: "int", nullable: false),
                    AchievedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayScores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayScores_OriginalGameId_Score",
                table: "ReplayScores",
                columns: new[] { "OriginalGameId", "Score" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayScores_OriginalGameId_UserId",
                table: "ReplayScores",
                columns: new[] { "OriginalGameId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReplayScores");

            migrationBuilder.DropColumn(
                name: "ReplayOfGameId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Seconds",
                table: "Games");
        }
    }
}
