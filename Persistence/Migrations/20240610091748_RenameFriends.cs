using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameFriends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriends_AspNetUsers_FriendId",
                table: "UserFriends");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriends_AspNetUsers_UserId",
                table: "UserFriends");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserFriends",
                newName: "SecondUserId");

            migrationBuilder.RenameColumn(
                name: "FriendId",
                table: "UserFriends",
                newName: "FirstUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFriends_UserId",
                table: "UserFriends",
                newName: "IX_UserFriends_SecondUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFriends_FriendId",
                table: "UserFriends",
                newName: "IX_UserFriends_FirstUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriends_AspNetUsers_FirstUserId",
                table: "UserFriends",
                column: "FirstUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriends_AspNetUsers_SecondUserId",
                table: "UserFriends",
                column: "SecondUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriends_AspNetUsers_FirstUserId",
                table: "UserFriends");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriends_AspNetUsers_SecondUserId",
                table: "UserFriends");

            migrationBuilder.RenameColumn(
                name: "SecondUserId",
                table: "UserFriends",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FirstUserId",
                table: "UserFriends",
                newName: "FriendId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFriends_SecondUserId",
                table: "UserFriends",
                newName: "IX_UserFriends_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFriends_FirstUserId",
                table: "UserFriends",
                newName: "IX_UserFriends_FriendId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriends_AspNetUsers_FriendId",
                table: "UserFriends",
                column: "FriendId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriends_AspNetUsers_UserId",
                table: "UserFriends",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
