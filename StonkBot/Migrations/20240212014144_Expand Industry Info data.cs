using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class ExpandIndustryInfodata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EarningsPerShare",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarningsPerShare",
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
        }
    }
}
