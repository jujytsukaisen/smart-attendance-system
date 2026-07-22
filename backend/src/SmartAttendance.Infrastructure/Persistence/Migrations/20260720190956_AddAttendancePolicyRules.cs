using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAttendance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendancePolicyRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoCheckoutEnabled",
                table: "AttendancePolicies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "AutoCheckoutTime",
                table: "AttendancePolicies",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EarlyLeaveToleranceMinutes",
                table: "AttendancePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GracePeriodMinutes",
                table: "AttendancePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HolidayPolicy",
                table: "AttendancePolicies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Regular");

            migrationBuilder.AddColumn<int>(
                name: "LateToleranceMinutes",
                table: "AttendancePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaximumOvertimeMinutes",
                table: "AttendancePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimumOvertimeMinutes",
                table: "AttendancePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MissingPunchPolicy",
                table: "AttendancePolicies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Reject");

            migrationBuilder.AddColumn<bool>(
                name: "MultipleShiftsSupport",
                table: "AttendancePolicies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeThresholdMinutes",
                table: "AttendancePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RoundTimeRule",
                table: "AttendancePolicies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "WeekendPolicy",
                table: "AttendancePolicies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Regular");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoCheckoutEnabled",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "AutoCheckoutTime",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "EarlyLeaveToleranceMinutes",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "GracePeriodMinutes",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "HolidayPolicy",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "LateToleranceMinutes",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "MaximumOvertimeMinutes",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "MinimumOvertimeMinutes",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "MissingPunchPolicy",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "MultipleShiftsSupport",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "OvertimeThresholdMinutes",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "RoundTimeRule",
                table: "AttendancePolicies");

            migrationBuilder.DropColumn(
                name: "WeekendPolicy",
                table: "AttendancePolicies");
        }
    }
}
