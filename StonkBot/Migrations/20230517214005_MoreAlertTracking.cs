using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class MoreAlertTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EarningsReports_Symbol_Date",
                table: "EarningsReports");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "EarningsReports",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "EarningsReports",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "HighHalfAlert",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HighThirdAlert",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LowHalfAlert",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LowThirdAlert",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighHalfAlert",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "HighThirdAlert",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "LowHalfAlert",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "LowThirdAlert",
                table: "EarningsReports");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "EarningsReports",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "EarningsReports",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.CreateIndex(
                name: "IX_EarningsReports_Symbol_Date",
                table: "EarningsReports",
                columns: new[] { "Symbol", "Date" });
        }
    }
}
