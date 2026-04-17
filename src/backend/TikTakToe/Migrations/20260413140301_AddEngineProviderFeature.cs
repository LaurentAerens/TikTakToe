using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTakToe.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineProviderFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "game_id",
                table: "players",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "normalized_display_name",
                table: "engine_capabilities",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_engine_template_external_id",
                table: "players",
                column: "external_id",
                unique: true,
                filter: "\"is_engine\" = TRUE AND \"game_id\" IS NULL AND \"external_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_engine_capabilities_normalized_display_name",
                table: "engine_capabilities",
                column: "normalized_display_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_engine_template_external_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_engine_capabilities_normalized_display_name",
                table: "engine_capabilities");

            migrationBuilder.DropColumn(
                name: "normalized_display_name",
                table: "engine_capabilities");

            migrationBuilder.AlterColumn<Guid>(
                name: "game_id",
                table: "players",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
