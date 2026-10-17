    using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppBulkMessaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateBodyAndHeaderFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyText",
                schema: "erpsystem",
                table: "WhatsAppTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderFormat",
                schema: "erpsystem",
                table: "WhatsAppTemplates",
                type: "nvarchar(max)",
                nullable: true);

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppReplies",
                schema: "erpsystem");

            migrationBuilder.DropColumn(
                name: "BodyText",
                schema: "erpsystem",
                table: "WhatsAppTemplates");

            migrationBuilder.DropColumn(
                name: "HeaderFormat",
                schema: "erpsystem",
                table: "WhatsAppTemplates");
        }
    }
}
