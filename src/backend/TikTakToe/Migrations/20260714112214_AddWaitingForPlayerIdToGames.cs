using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTakToe.Migrations
{
    /// <inheritdoc />
    public partial class AddWaitingForPlayerIdToGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "waiting_for_player_id",
                table: "games",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "waiting_for_player_id",
                table: "games");
        }
    }
}
