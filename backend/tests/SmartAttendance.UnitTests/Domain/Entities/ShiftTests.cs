using System;
using System.Collections.Generic;
using FluentAssertions;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;
using Xunit;

namespace SmartAttendance.UnitTests.Domain.Entities;

public class ShiftTests
{
    private Shift CreateTestShift()
    {
        return Shift.Create(
            companyId: Guid.NewGuid(),
            workScheduleId: Guid.NewGuid(),
            name: "Morning Shift",
            code: "MORN",
            description: "Regular morning shift",
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(17, 0),
            shiftType: ShiftType.Morning,
            isFlexible: false,
            minimumWorkingHours: 7,
            maximumWorkingHours: 9,
            gracePeriodMinutes: 15,
            allowEarlyCheckInMinutes: 30,
            allowLateCheckOutMinutes: 30,
            maxBreakDurationMinutes: 60);
    }

    [Fact]
    public void Create_ShouldThrowInvalidShiftException_WhenExceedingWorkingHours()
    {
        // Act
        Action act = () => Shift.Create(
            companyId: Guid.NewGuid(),
            workScheduleId: Guid.NewGuid(),
            name: "Invalid Shift",
            code: "INV",
            description: null,
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(17, 0),
            shiftType: ShiftType.Morning,
            isFlexible: false,
            minimumWorkingHours: 9,
            maximumWorkingHours: 7, // Invalid: Max < Min
            gracePeriodMinutes: 15,
            allowEarlyCheckInMinutes: 30,
            allowLateCheckOutMinutes: 30,
            maxBreakDurationMinutes: 60);

        // Assert
        act.Should().Throw<InvalidShiftException>()
            .WithMessage("Maximum working hours must be greater than or equal to minimum working hours.");
    }

    [Fact]
    public void AddBreak_ShouldThrowInvalidShiftException_WhenOverlappingBreaks()
    {
        // Arrange
        var shift = CreateTestShift();
        var break1 = Break.Create(Guid.NewGuid(), shift.Id, "Lunch", null, new TimeOnly(12, 0), new TimeOnly(13, 0), 60, BreakType.Lunch, true, true);
        var break2 = Break.Create(Guid.NewGuid(), shift.Id, "Meeting", null, new TimeOnly(12, 30), new TimeOnly(13, 30), 60, BreakType.Rest, false, true);
        shift.AddBreak(break1);

        // Act
        Action act = () => shift.AddBreak(break2);

        // Assert
        act.Should().Throw<InvalidShiftException>()
            .WithMessage("Breaks cannot overlap.");
    }
}
