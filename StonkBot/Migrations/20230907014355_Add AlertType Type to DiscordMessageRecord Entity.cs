using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertTypeTypetoDiscordMessageRecordEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "DiscordMessageRecords",
                newName: "DateTime");

            migrationBuilder.RenameIndex(
                name: "IX_DiscordMessageRecords_Channel_Date",
                table: "DiscordMessageRecords",
                newName: "IX_DiscordMessageRecords_Channel_DateTime");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "DiscordMessageRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "DiscordMessageRecords");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "DiscordMessageRecords",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_DiscordMessageRecords_Channel_DateTime",
                table: "DiscordMessageRecords",
                newName: "IX_DiscordMessageRecords_Channel_Date");
        }
    }
}
