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
}

