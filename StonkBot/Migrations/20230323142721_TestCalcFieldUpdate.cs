using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class TestCalcFieldUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalData_IndustryInfo_Symbol",
                table: "HistoricalData");

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

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalData_IndustryInfo_Symbol",
                table: "HistoricalData",
                column: "Symbol",
                principalTable: "IndustryInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
