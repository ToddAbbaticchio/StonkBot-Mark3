using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class RenameErAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alert");

            migrationBuilder.CreateTable(
                name: "ErAlert",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    EarningsReportDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EarningsReportSymbol = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErAlert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErAlert_EarningsReports_EarningsReportSymbol_EarningsReportDate",
                        columns: x => new { x.EarningsReportSymbol, x.EarningsReportDate },
                        principalTable: "EarningsReports",
                        principalColumns: new[] { "Symbol", "Date" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErAlert_EarningsReportSymbol_EarningsReportDate",
                table: "ErAlert",
                columns: new[] { "EarningsReportSymbol", "EarningsReportDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErAlert");

            migrationBuilder.CreateTable(
                name: "Alert",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EarningsReportDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EarningsReportSymbol = table.Column<string>(type: "TEXT", nullable: true),
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
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
    }
}
