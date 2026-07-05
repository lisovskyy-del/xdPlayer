using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xdPlayer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMusicBrainzEnabledToUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MusicBrainzEnabled",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MusicBrainzEnabled",
                table: "UserProfiles");
        }
    }
}
