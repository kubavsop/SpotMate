using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Interests");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Interests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Interests");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Interests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
