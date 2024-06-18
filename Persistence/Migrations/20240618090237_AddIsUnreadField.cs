using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUnreadField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnreadMessagesCount",
                table: "ChatUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsUnread",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnread",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "UnreadMessagesCount",
                table: "ChatUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
