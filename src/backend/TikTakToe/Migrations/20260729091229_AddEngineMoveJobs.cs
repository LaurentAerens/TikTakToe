using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTakToe.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineMoveJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine_move_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    available_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    lease_owner = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    lease_expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_move_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_engine_move_jobs_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_move_jobs_available_created",
                table: "engine_move_jobs",
                columns: new[] { "available_at_utc", "created_at_utc" },
                filter: "\"status\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "UIX_engine_move_jobs_pending_game_id",
                table: "engine_move_jobs",
                column: "game_id",
                unique: true,
                filter: "\"status\" = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_move_jobs");
        }
    }
}
