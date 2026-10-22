using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppBulkMessaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWhatsAppReplyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatsAppReplies",
                schema: "erpsystem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetaMessageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplyMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplyReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WhatsAppMessageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppReplies", x => x.Id);
                });
        }
    }
}
