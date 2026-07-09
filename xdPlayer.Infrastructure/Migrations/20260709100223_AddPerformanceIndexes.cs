using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xdPlayer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlaylistTracks_PlaylistId_Position",
                table: "PlaylistTracks",
                columns: new[] { "PlaylistId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_ListeningSessions_StartedAt",
                table: "ListeningSessions",
                column: "StartedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlaylistTracks_PlaylistId_Position",
                table: "PlaylistTracks");

            migrationBuilder.DropIndex(
                name: "IX_ListeningSessions_StartedAt",
                table: "ListeningSessions");
        }
    }
}
