using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTakToe.Migrations
{
    /// <inheritdoc />
    public partial class AddBotsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    engine_capability_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    depth = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bots", x => x.id);
                    table.ForeignKey(
                        name: "FK_bots_engine_capabilities_engine_capability_id",
                        column: x => x.engine_capability_id,
                        principalTable: "engine_capabilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bots_players_id",
                        column: x => x.id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bots_engine_capability_id",
                table: "bots",
                column: "engine_capability_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bots");
        }
    }
}
