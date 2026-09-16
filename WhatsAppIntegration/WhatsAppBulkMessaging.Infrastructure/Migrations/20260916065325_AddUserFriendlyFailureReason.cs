using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppBulkMessaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFriendlyFailureReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserFriendlyFailureReason",
                schema: "erpsystem",
                table: "tblwhatsappmessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserFriendlyFailureReason",
                schema: "erpsystem",
                table: "tblwhatsappmessages");
        }
    }
}
