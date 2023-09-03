using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class SplitErNotificationsOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IpoListings_Symbol",
                table: "IpoListings");

            migrationBuilder.DropIndex(
                name: "IX_IpoHData_Symbol_Date",
                table: "IpoHData");

            migrationBuilder.DropIndex(
                name: "IX_IndustryInfo_Symbol",
                table: "IndustryInfo");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalData_Symbol_Date",
                table: "HistoricalData");

            migrationBuilder.DropIndex(
                name: "IX_EsCandles_ChartTime",
                table: "EsCandles");

            migrationBuilder.DropIndex(
                name: "IX_CalculatedFields_Symbol_Date",
                table: "CalculatedFields");

            migrationBuilder.DropColumn(
                name: "ErHighDate",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "ErLowDate",
                table: "EarningsReports");

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

            migrationBuilder.DropColumn(
                name: "NegStartAlert1",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "NegStartAlert2",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "NegSureStartAlert1",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "NegSureStartAlert2",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "StartAlert1",
                table: "EarningsReports");

            migrationBuilder.DropColumn(
                name: "StartAlert2",
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

            migrationBuilder.CreateTable(
                name: "Alert",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    EarningsReportDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EarningsReportSymbol = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alert_EarningsReports_EarningsReportSymbol_EarningsReportDate",
                        columns: x => new { x.EarningsReportSymbol, x.EarningsReportDate },
                        principalTable: "EarningsReports",
                        principalColumns: new[] { "Symbol", "Date" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_EarningsReportSymbol_EarningsReportDate",
                table: "Alert",
                columns: new[] { "EarningsReportSymbol", "EarningsReportDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alert");

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
                name: "StartAlert1",
                table: "EarningsReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartAlert2",
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

            migrationBuilder.CreateIndex(
                name: "IX_IpoListings_Symbol",
                table: "IpoListings",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_IpoHData_Symbol_Date",
                table: "IpoHData",
                columns: new[] { "Symbol", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_IndustryInfo_Symbol",
                table: "IndustryInfo",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalData_Symbol_Date",
                table: "HistoricalData",
                columns: new[] { "Symbol", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_EsCandles_ChartTime",
                table: "EsCandles",
                column: "ChartTime");

            migrationBuilder.CreateIndex(
                name: "IX_CalculatedFields_Symbol_Date",
                table: "CalculatedFields",
                columns: new[] { "Symbol", "Date" });
        }
    }
}
