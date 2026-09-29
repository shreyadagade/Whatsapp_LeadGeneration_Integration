using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppBulkMessaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipientReplyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReplyMessage",
                schema: "erpsystem",
                table: "WhatsAppMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReplyReceivedAt",
                schema: "erpsystem",
                table: "WhatsAppMessages",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplyMessage",
                schema: "erpsystem",
                table: "WhatsAppMessages");

            migrationBuilder.DropColumn(
                name: "ReplyReceivedAt",
                schema: "erpsystem",
                table: "WhatsAppMessages");
        }
    }
}
