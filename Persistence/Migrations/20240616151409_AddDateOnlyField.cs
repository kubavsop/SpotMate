using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOnlyField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "DailySteps",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "DailySteps");
        }
    }
}
