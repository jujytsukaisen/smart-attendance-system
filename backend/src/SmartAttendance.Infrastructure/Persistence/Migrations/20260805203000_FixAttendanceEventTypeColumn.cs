using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAttendance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAttendanceEventTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns c
                    JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID('AttendanceEvents') 
                      AND c.name = 'EventType' 
                      AND t.name = 'int'
                )
                BEGIN
                    ALTER TABLE [AttendanceEvents] ALTER COLUMN [EventType] NVARCHAR(30) NOT NULL;
                    UPDATE [AttendanceEvents] SET [EventType] = CASE [EventType]
                        WHEN '1' THEN 'CheckIn'
                        WHEN '2' THEN 'BreakStart'
                        WHEN '3' THEN 'BreakEnd'
                        WHEN '4' THEN 'CheckOut'
                        ELSE [EventType]
                    END;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EventType",
                table: "AttendanceEvents",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
