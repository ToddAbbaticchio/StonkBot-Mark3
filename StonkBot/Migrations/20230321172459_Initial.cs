using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthTokens",
                columns: table => new
                {
                    access_token = table.Column<string>(type: "TEXT", nullable: false),
                    refresh_token = table.Column<string>(type: "TEXT", nullable: false),
                    token_type = table.Column<string>(type: "TEXT", nullable: false),
                    scope = table.Column<string>(type: "TEXT", nullable: false),
                    tokenCreatedTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    expires_in = table.Column<int>(type: "INTEGER", nullable: false),
                    refresh_token_expires_in = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.access_token);
                });

            migrationBuilder.CreateTable(
                name: "CalculatedFields",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FromYesterday = table.Column<string>(type: "TEXT", nullable: true),
                    UpToday = table.Column<bool>(type: "INTEGER", nullable: true),
                    VolumeAlert = table.Column<string>(type: "TEXT", nullable: true),
                    VolumeAlert2 = table.Column<string>(type: "TEXT", nullable: true),
                    FiveDayStable = table.Column<string>(type: "TEXT", nullable: true),
                    UpperShadow = table.Column<bool>(type: "INTEGER", nullable: true),
                    AboveUpperShadow = table.Column<string>(type: "TEXT", nullable: true),
                    FHTargetDay = table.Column<string>(type: "TEXT", nullable: true),
                    LastFHTarget = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculatedFields", x => new { x.Symbol, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "Candles",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Open = table.Column<decimal>(type: "TEXT", nullable: false),
                    Close = table.Column<decimal>(type: "TEXT", nullable: false),
                    Low = table.Column<decimal>(type: "TEXT", nullable: false),
                    High = table.Column<decimal>(type: "TEXT", nullable: false),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candles", x => new { x.Symbol, x.DateTime });
                });

            migrationBuilder.CreateTable(
                name: "DiscordMessageRecords",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WebhookUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordMessageRecords", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "EarningsReports",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEnding = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Time = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarningsReports", x => new { x.Symbol, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "IndustryInfo",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Sector = table.Column<string>(type: "TEXT", nullable: true),
                    Industry = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryInfo", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "IpoListings",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OfferingPrice = table.Column<string>(type: "TEXT", nullable: false),
                    OfferAmmount = table.Column<string>(type: "TEXT", nullable: true),
                    OfferingEndDate = table.Column<string>(type: "TEXT", nullable: true),
                    ExpectedListingDate = table.Column<string>(type: "TEXT", nullable: false),
                    ScrapeDate = table.Column<string>(type: "TEXT", nullable: false),
                    Open = table.Column<decimal>(type: "TEXT", nullable: true),
                    Close = table.Column<decimal>(type: "TEXT", nullable: true),
                    Low = table.Column<decimal>(type: "TEXT", nullable: true),
                    High = table.Column<decimal>(type: "TEXT", nullable: true),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: true),
                    DiscordMessageRecordMessageId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpoListings", x => x.Symbol);
                    table.ForeignKey(
                        name: "FK_IpoListings_DiscordMessageRecords_DiscordMessageRecordMessageId",
                        column: x => x.DiscordMessageRecordMessageId,
                        principalTable: "DiscordMessageRecords",
                        principalColumn: "MessageId");
                });

            migrationBuilder.CreateTable(
                name: "HistoricalData",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Open = table.Column<decimal>(type: "TEXT", nullable: false),
                    High = table.Column<decimal>(type: "TEXT", nullable: false),
                    Low = table.Column<decimal>(type: "TEXT", nullable: false),
                    Close = table.Column<decimal>(type: "TEXT", nullable: false),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: false),
                    CalculatedFieldsSymbol = table.Column<string>(type: "TEXT", nullable: true),
                    CalculatedFieldsDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalData", x => new { x.Symbol, x.Date });
                    table.ForeignKey(
                        name: "FK_HistoricalData_CalculatedFields_CalculatedFieldsSymbol_CalculatedFieldsDate",
                        columns: x => new { x.CalculatedFieldsSymbol, x.CalculatedFieldsDate },
                        principalTable: "CalculatedFields",
                        principalColumns: new[] { "Symbol", "Date" });
                    table.ForeignKey(
                        name: "FK_HistoricalData_IndustryInfo_Symbol",
                        column: x => x.Symbol,
                        principalTable: "IndustryInfo",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_access_token",
                table: "AuthTokens",
                column: "access_token");

            migrationBuilder.CreateIndex(
                name: "IX_CalculatedFields_Symbol_Date",
                table: "CalculatedFields",
                columns: new[] { "Symbol", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Candles_Symbol_DateTime",
                table: "Candles",
                columns: new[] { "Symbol", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessageRecords_MessageId",
                table: "DiscordMessageRecords",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EarningsReports_Symbol_Date",
                table: "EarningsReports",
                columns: new[] { "Symbol", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalData_CalculatedFieldsSymbol_CalculatedFieldsDate",
                table: "HistoricalData",
                columns: new[] { "CalculatedFieldsSymbol", "CalculatedFieldsDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalData_Symbol_Date",
                table: "HistoricalData",
                columns: new[] { "Symbol", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_IndustryInfo_Symbol",
                table: "IndustryInfo",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_IpoListings_DiscordMessageRecordMessageId",
                table: "IpoListings",
                column: "DiscordMessageRecordMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_IpoListings_Symbol",
                table: "IpoListings",
                column: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthTokens");

            migrationBuilder.DropTable(
                name: "Candles");

            migrationBuilder.DropTable(
                name: "EarningsReports");

            migrationBuilder.DropTable(
                name: "HistoricalData");

            migrationBuilder.DropTable(
                name: "IpoListings");

            migrationBuilder.DropTable(
                name: "CalculatedFields");

            migrationBuilder.DropTable(
                name: "IndustryInfo");

            migrationBuilder.DropTable(
                name: "DiscordMessageRecords");
        }
    }
}
