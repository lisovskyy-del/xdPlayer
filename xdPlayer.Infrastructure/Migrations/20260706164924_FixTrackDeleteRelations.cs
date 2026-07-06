using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xdPlayer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTrackDeleteRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyStatistics_Tracks_TopTrackId",
                table: "DailyStatistics");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyStatistics_Tracks_TopTrackId",
                table: "DailyStatistics",
                column: "TopTrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyStatistics_Tracks_TopTrackId",
                table: "DailyStatistics");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyStatistics_Tracks_TopTrackId",
                table: "DailyStatistics",
                column: "TopTrackId",
                principalTable: "Tracks",
                principalColumn: "Id");
        }
    }
}
