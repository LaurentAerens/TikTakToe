using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTakToe.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingMoveColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MoveNumber",
                table: "moves",
                newName: "move_number");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "moves",
                newName: "created_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_moves_game_id_MoveNumber",
                table: "moves",
                newName: "IX_moves_game_id_move_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "move_number",
                table: "moves",
                newName: "MoveNumber");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "moves",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_moves_game_id_move_number",
                table: "moves",
                newName: "IX_moves_game_id_MoveNumber");
        }
    }
}
