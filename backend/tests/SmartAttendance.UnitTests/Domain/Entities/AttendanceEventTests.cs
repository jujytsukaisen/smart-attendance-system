using System;
using FluentAssertions;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using Xunit;

namespace SmartAttendance.UnitTests.Domain.Entities;

public class AttendanceEventTests
{
    [Fact]
    public void Create_ShouldInitializeAttendanceEvent_WithCorrectProperties()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var attendanceId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var eventType = AttendanceEventType.CheckIn;
        var note = "Morning scan";

        // Act
        var evt = AttendanceEvent.Create(companyId, attendanceId, employeeId, timestamp, eventType, note);

        // Assert
        evt.Id.Should().NotBeEmpty();
        evt.CompanyId.Should().Be(companyId);
        evt.AttendanceId.Should().Be(attendanceId);
        evt.EmployeeId.Should().Be(employeeId);
        evt.Timestamp.Should().Be(timestamp);
        evt.EventType.Should().Be(eventType);
        evt.Note.Should().Be(note);
    }

    [Theory]
    [InlineData(AttendanceEventType.CheckIn)]
    [InlineData(AttendanceEventType.BreakStart)]
    [InlineData(AttendanceEventType.BreakEnd)]
    [InlineData(AttendanceEventType.CheckOut)]
    public void Create_ShouldSupportAllEventTypes(AttendanceEventType eventType)
    {
        // Arrange & Act
        var evt = AttendanceEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            eventType);

        // Assert
        evt.EventType.Should().Be(eventType);
    }
}
