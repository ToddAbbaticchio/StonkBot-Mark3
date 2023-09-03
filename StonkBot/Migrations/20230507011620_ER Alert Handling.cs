using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class ERAlertHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ErHighDate",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ErLowDate",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NegStartAlert1",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NegStartAlert2",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartAlert1",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartAlert2",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErHighDate",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "ErLowDate",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "NegStartAlert1",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "NegStartAlert2",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "StartAlert1",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "StartAlert2",
                table: "EarningsReports");
        }
    }
}
