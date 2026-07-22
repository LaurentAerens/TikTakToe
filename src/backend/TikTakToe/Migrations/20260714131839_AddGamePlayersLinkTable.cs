using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTakToe.Migrations
{
    /// <inheritdoc />
    public partial class AddGamePlayersLinkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_games_game_id",
                table: "players");

            migrationBuilder.CreateTable(
                name: "game_players",
                columns: table => new
                {
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    turn_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_players", x => new { x.game_id, x.player_id });
                    table.ForeignKey(
                        name: "FK_game_players_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_players_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_players_game_id_turn_order",
                table: "game_players",
                columns: new[] { "game_id", "turn_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_players_player_id",
                table: "game_players",
                column: "player_id");

            migrationBuilder.AddForeignKey(
                name: "FK_players_games_game_id",
                table: "players",
                column: "game_id",
                principalTable: "games",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_games_game_id",
                table: "players");

            migrationBuilder.DropTable(
                name: "game_players");

            migrationBuilder.AddForeignKey(
                name: "FK_players_games_game_id",
                table: "players",
                column: "game_id",
                principalTable: "games",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
