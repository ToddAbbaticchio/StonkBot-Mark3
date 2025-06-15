using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class RevertaddDateandSplitIndustryInfotable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndustryInfo_HistoricalData_Symbol_Date",
                table: "IndustryInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IndustryInfo",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "EarningsPerShare",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "MarketCap",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "NetIncome",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "RelativeVolume",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "VolatilityM",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "VolatilityW",
                table: "IndustryInfo");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "IndustryInfo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IndustryInfo",
                table: "IndustryInfo",
                column: "Symbol");

            migrationBuilder.CreateTable(
                name: "IndustryInfoHData",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: true),
                    RelativeVolume = table.Column<decimal>(type: "TEXT", nullable: true),
                    EarningsPerShare = table.Column<decimal>(type: "TEXT", nullable: true),
                    VolatilityW = table.Column<decimal>(type: "TEXT", nullable: true),
                    VolatilityM = table.Column<decimal>(type: "TEXT", nullable: true),
                    TotalRevenue = table.Column<decimal>(type: "TEXT", nullable: true),
                    NetIncome = table.Column<decimal>(type: "TEXT", nullable: true),
                    MarketCap = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryInfoHData", x => new { x.Symbol, x.Date });
                    table.ForeignKey(
                        name: "FK_IndustryInfoHData_IndustryInfo_Symbol",
                        column: x => x.Symbol,
                        principalTable: "IndustryInfo",
                        principalColumn: "Symbol");
                });

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalData_IndustryInfo_Symbol",
                table: "HistoricalData",
                column: "Symbol",
                principalTable: "IndustryInfo",
                principalColumn: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalData_IndustryInfo_Symbol",
                table: "HistoricalData");

            migrationBuilder.DropTable(
                name: "IndustryInfoHData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IndustryInfo",
                table: "IndustryInfo");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<decimal>(
                name: "EarningsPerShare",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarketCap",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetIncome",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RelativeVolume",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VolatilityM",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VolatilityW",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                table: "IndustryInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IndustryInfo",
                table: "IndustryInfo",
                columns: new[] { "Symbol", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryInfo_HistoricalData_Symbol_Date",
                table: "IndustryInfo",
                columns: new[] { "Symbol", "Date" },
                principalTable: "HistoricalData",
                principalColumns: new[] { "Symbol", "Date" });
        }
    }
}
