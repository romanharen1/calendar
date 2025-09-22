using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Art.OnShift.Scheduler.Migrations
{
    /// <inheritdoc />
    public partial class NotificationHandler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlertMinutesBefore",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AlertSent",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApiNotificationSent",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertMinutesBefore",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AlertSent",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApiNotificationSent",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Events");
        }
    }
}
