using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppBulkMessaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppChatMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatsAppChatMessages",
                schema: "erpsystem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MetaMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppChatMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppChatMessages_MetaMessageId",
                schema: "erpsystem",
                table: "WhatsAppChatMessages",
                column: "MetaMessageId",
                unique: true,
                filter: "[MetaMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppChatMessages_PhoneNumber",
                schema: "erpsystem",
                table: "WhatsAppChatMessages",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppChatMessages",
                schema: "erpsystem");
        }
    }
}
