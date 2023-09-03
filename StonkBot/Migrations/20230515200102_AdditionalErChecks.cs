using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalErChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NegSureStartAlert1",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NegSureStartAlert2",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SureErHighDate",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SureErLowDate",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SureStartAlert1",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SureStartAlert2",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NegSureStartAlert1",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "NegSureStartAlert2",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "SureErHighDate",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "SureErLowDate",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "SureStartAlert1",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "SureStartAlert2",
                table: "EarningsReports");
        }
    }
}
