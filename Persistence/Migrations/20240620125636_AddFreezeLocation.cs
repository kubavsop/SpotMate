using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFreezeLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocationFrozen",
                table: "UserFriends");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "UserFriends");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "UserFriends");

            migrationBuilder.CreateTable(
                name: "FreezeLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FreezerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsLocationFrozen = table.Column<bool>(type: "boolean", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreezeLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FreezeLocations_AspNetUsers_FreezerUserId",
                        column: x => x.FreezerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FreezeLocations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FreezeLocations_FreezerUserId",
                table: "FreezeLocations",
                column: "FreezerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FreezeLocations_UserId",
                table: "FreezeLocations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreezeLocations");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocationFrozen",
                table: "UserFriends",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "UserFriends",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "UserFriends",
                type: "double precision",
                nullable: true);
        }
    }
}
