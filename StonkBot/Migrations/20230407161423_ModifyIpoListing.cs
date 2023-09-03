using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StonkBot.Migrations
{
    /// <inheritdoc />
    public partial class ModifyIpoListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IpoListings_DiscordMessageRecords_DiscordMessageRecordMessageId",
                table: "IpoListings");

            migrationBuilder.DropIndex(
                name: "IX_IpoListings_DiscordMessageRecordMessageId",
                table: "IpoListings");

            migrationBuilder.DropColumn(
                name: "DiscordMessageRecordMessageId",
                table: "IpoListings");

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstPassDate",
                table: "IpoListings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSecondPassDate",
                table: "IpoListings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstPassDate",
                table: "IpoListings");

            migrationBuilder.DropColumn(
                name: "LastSecondPassDate",
                table: "IpoListings");

            migrationBuilder.AddColumn<ulong>(
                name: "DiscordMessageRecordMessageId",
                table: "IpoListings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IpoListings_DiscordMessageRecordMessageId",
                table: "IpoListings",
                column: "DiscordMessageRecordMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_IpoListings_DiscordMessageRecords_DiscordMessageRecordMessageId",
                table: "IpoListings",
                column: "DiscordMessageRecordMessageId",
                principalTable: "DiscordMessageRecords",
                principalColumn: "MessageId");
        }
    }
}
