using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDiscordMessageRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiscordMessageRecords_MessageId",
                table: "DiscordMessageRecords");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "DiscordMessageRecords");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "DiscordMessageRecords",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessageRecords_Channel_Date",
                table: "DiscordMessageRecords",
                columns: new[] { "Channel", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiscordMessageRecords_Channel_Date",
                table: "DiscordMessageRecords");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "DiscordMessageRecords");

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "DiscordMessageRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessageRecords_MessageId",
                table: "DiscordMessageRecords",
                column: "MessageId");
        }
    }
}
