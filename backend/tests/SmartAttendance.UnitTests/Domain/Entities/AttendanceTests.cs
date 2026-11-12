using System;
using FluentAssertions;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;
using Xunit;

namespace SmartAttendance.UnitTests.Domain.Entities;

public class AttendanceTests
{
    private Attendance CreateTestAttendance(DateTime checkInTime)
    {
        return Attendance.CreateCheckIn(
            companyId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            attendanceDeviceId: Guid.NewGuid(),
            attendancePolicyId: Guid.NewGuid(),
            shiftId: Guid.NewGuid(),
            checkInTime: checkInTime,
            checkInMethod: CheckInMethod.FaceRecognition);
    }

    [Fact]
    public void RecordCheckOut_ShouldThrowInvalidAttendanceException_WhenCheckingOutTwice()
    {
        // Arrange
        var checkInTime = DateTime.UtcNow.AddHours(-4);
        var attendance = CreateTestAttendance(checkInTime);
        var checkOutTime = DateTime.UtcNow;
        
        attendance.RecordCheckOut(checkOutTime, CheckInMethod.FaceRecognition);

        // Act
        Action act = () => attendance.RecordCheckOut(checkOutTime.AddHours(1), CheckInMethod.FaceRecognition);

        // Assert
        act.Should().Throw<InvalidAttendanceException>()
            .WithMessage("Cannot check out twice.");
    }

    [Fact]
    public void RecordCheckOut_ShouldThrowInvalidAttendanceException_WhenCheckOutBeforeCheckIn()
    {
        // Arrange
        var checkInTime = DateTime.UtcNow;
        var attendance = CreateTestAttendance(checkInTime);
        var invalidCheckOutTime = checkInTime.AddHours(-1); // Before check-in

        // Act
        Action act = () => attendance.RecordCheckOut(invalidCheckOutTime, CheckInMethod.FaceRecognition);

        // Assert
        act.Should().Throw<InvalidAttendanceException>()
            .WithMessage("Check-out cannot occur before check-in.");
    }

    [Fact]
    public void CalculateWorkingHours_ShouldSetLateMinutes_WhenLateIsProvided()
    {
        // Arrange
        var checkInTime = DateTime.UtcNow.Date.AddHours(9).AddMinutes(15); // 9:15 AM
        var checkOutTime = DateTime.UtcNow.Date.AddHours(17); // 5:00 PM
        var attendance = CreateTestAttendance(checkInTime);
        attendance.RecordCheckOut(checkOutTime, CheckInMethod.FaceRecognition);

        var lateMinutes = 15;
        var earlyLeaveMinutes = 0;
        var breakMinutes = 60;
        var overtimeMinutes = 0;

        // Act
        attendance.CalculateWorkingHours(lateMinutes, earlyLeaveMinutes, breakMinutes, overtimeMinutes);

        // Assert
        attendance.LateMinutes.Should().Be(15);
        attendance.WorkingMinutes.Should().Be(405); // (17-9.25) = 7.75 hrs = 465 mins - 60 break = 405
        attendance.AttendanceStatus.Should().Be(AttendanceStatus.Late); // Based on domain placeholder
    }

    [Fact]
    public void UpdateSummaryFromEvents_ShouldCalculateBreakAndWorkingMinutes_Correctly()
    {
        // Arrange: CheckIn at 09:00, Break 12:00-12:30 (30m), Break 15:00-15:15 (15m), CheckOut at 17:00 (Total span 8h = 480m)
        var baseDate = new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc);
        var attendance = CreateTestAttendance(baseDate);
        var checkOutTime = baseDate.AddHours(8); // 17:00

        var events = new List<AttendanceEvent>
        {
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, baseDate, AttendanceEventType.CheckIn),
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, baseDate.AddHours(3), AttendanceEventType.BreakStart), // 12:00
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, baseDate.AddHours(3.5), AttendanceEventType.BreakEnd), // 12:30 (30 min)
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, baseDate.AddHours(6), AttendanceEventType.BreakStart), // 15:00
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, baseDate.AddHours(6.25), AttendanceEventType.BreakEnd), // 15:15 (15 min)
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, checkOutTime, AttendanceEventType.CheckOut), // 17:00
        };

        // Act
        attendance.RecordCheckOutFromEvent(checkOutTime, CheckInMethod.FaceRecognition);
        attendance.UpdateSummaryFromEvents(events);

        // Assert: total breaks = 45m, total working = 480 - 45 = 435m
        attendance.BreakMinutes.Should().Be(45);
        attendance.WorkingMinutes.Should().Be(435);
    }

    [Fact]
    public void RecordCheckOutFromEvent_ShouldSetCheckOutTimeAndRecalculate()
    {
        // Arrange
        var checkInTime = new DateTime(2026, 8, 5, 8, 0, 0, DateTimeKind.Utc);
        var attendance = CreateTestAttendance(checkInTime);
        var checkOutTime = checkInTime.AddHours(9); // 17:00

        var events = new List<AttendanceEvent>
        {
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, checkInTime, AttendanceEventType.CheckIn),
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, checkInTime.AddHours(4), AttendanceEventType.BreakStart), // 12:00
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, checkInTime.AddHours(5), AttendanceEventType.BreakEnd), // 13:00 (60 min)
            AttendanceEvent.Create(attendance.CompanyId, attendance.Id, attendance.EmployeeId, checkOutTime, AttendanceEventType.CheckOut),
        };

        // Act
        attendance.RecordCheckOutFromEvent(checkOutTime, CheckInMethod.FaceRecognition);
        attendance.UpdateSummaryFromEvents(events);

        // Assert
        attendance.CheckOutTime.Should().Be(checkOutTime);
        attendance.BreakMinutes.Should().Be(60);
        attendance.WorkingMinutes.Should().Be(480); // 9h total (540m) - 60m break = 480m
        attendance.AttendanceStatus.Should().Be(AttendanceStatus.Present);
    }
}



