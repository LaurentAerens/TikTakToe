using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTakToe.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlayerGameIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_games_game_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_engine_template_external_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_game_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "game_id",
                table: "players");

            migrationBuilder.CreateIndex(
                name: "IX_players_engine_template_external_id",
                table: "players",
                column: "external_id",
                unique: true,
                filter: "\"is_engine\" = TRUE AND \"external_id\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_engine_template_external_id",
                table: "players");

            migrationBuilder.AddColumn<Guid>(
                name: "game_id",
                table: "players",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_engine_template_external_id",
                table: "players",
                column: "external_id",
                unique: true,
                filter: "\"is_engine\" = TRUE AND \"game_id\" IS NULL AND \"external_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_players_game_id",
                table: "players",
                column: "game_id");

            migrationBuilder.AddForeignKey(
                name: "FK_players_games_game_id",
                table: "players",
                column: "game_id",
                principalTable: "games",
                principalColumn: "id");
        }
    }
}
