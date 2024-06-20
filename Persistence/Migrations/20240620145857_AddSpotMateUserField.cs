using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpotMateUserField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInterestBasedLocationSharable",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInterestBasedLocationSharable",
                table: "AspNetUsers");
        }
    }
}
