using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAttendance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceDeviceRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommunicationProtocol",
                table: "AttendanceDevices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionStatus",
                table: "AttendanceDevices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AttendanceDevices",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "AttendanceDevices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsFingerprint",
                table: "AttendanceDevices",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommunicationProtocol",
                table: "AttendanceDevices");

            migrationBuilder.DropColumn(
                name: "ConnectionStatus",
                table: "AttendanceDevices");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AttendanceDevices");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "AttendanceDevices");

            migrationBuilder.DropColumn(
                name: "SupportsFingerprint",
                table: "AttendanceDevices");
        }
    }
}
