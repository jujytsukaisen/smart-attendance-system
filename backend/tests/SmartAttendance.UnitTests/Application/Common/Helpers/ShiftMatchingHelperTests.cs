using System;
using System.Collections.Generic;
using SmartAttendance.Application.Common.Helpers;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Common.Helpers;

public class ShiftMatchingHelperTests
{
    private readonly Shift _morningShift;
    private readonly Shift _eveningShift;
    private readonly Shift _nightShift;

    public ShiftMatchingHelperTests()
    {
        var companyId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();

        _morningShift = Shift.Create(
            companyId: companyId,
            workScheduleId: scheduleId,
            name: "Morning Shift",
            code: "MORN",
            description: "Morning",
            startTime: new TimeOnly(8, 0),
            endTime: new TimeOnly(16, 0),
            shiftType: ShiftType.Morning,
            isFlexible: false,
            minimumWorkingHours: 8,
            maximumWorkingHours: 8,
            gracePeriodMinutes: 15,
            allowEarlyCheckInMinutes: 30,
            allowLateCheckOutMinutes: 30,
            maxBreakDurationMinutes: 60);
        _morningShift.Activate();

        _eveningShift = Shift.Create(
            companyId: companyId,
            workScheduleId: scheduleId,
            name: "Evening Shift",
            code: "EVE",
            description: "Evening",
            startTime: new TimeOnly(16, 0),
            endTime: new TimeOnly(0, 0),
            shiftType: ShiftType.Afternoon,
            isFlexible: false,
            minimumWorkingHours: 8,
            maximumWorkingHours: 8,
            gracePeriodMinutes: 15,
            allowEarlyCheckInMinutes: 30,
            allowLateCheckOutMinutes: 30,
            maxBreakDurationMinutes: 60);
        _eveningShift.Activate();

        _nightShift = Shift.Create(
            companyId: companyId,
            workScheduleId: scheduleId,
            name: "Night Shift",
            code: "NIGHT",
            description: "Night",
            startTime: new TimeOnly(23, 0),
            endTime: new TimeOnly(7, 0),
            shiftType: ShiftType.Night,
            isFlexible: false,
            minimumWorkingHours: 8,
            maximumWorkingHours: 8,
            gracePeriodMinutes: 15,
            allowEarlyCheckInMinutes: 30,
            allowLateCheckOutMinutes: 30,
            maxBreakDurationMinutes: 60);
        _nightShift.Activate();
    }

    [Fact]
    public void DetectBestShift_ShouldReturnMorningShift_WhenScanTimeIsAroundMorningStart()
    {
        var shifts = new List<Shift> { _morningShift, _eveningShift, _nightShift };
        // 07:55 is right before morning shift (08:00) within early buffer
        var scanTime = new DateTime(2026, 8, 8, 7, 55, 0);

        var result = ShiftMatchingHelper.DetectBestShift(shifts, scanTime);

        Assert.NotNull(result);
        Assert.Equal(_morningShift.Id, result.Id);
    }

    [Fact]
    public void DetectBestShift_ShouldReturnEveningShift_WhenScanTimeIsAroundEveningStart()
    {
        var shifts = new List<Shift> { _morningShift, _eveningShift, _nightShift };
        // 16:10 is shortly after evening shift start (16:00)
        var scanTime = new DateTime(2026, 8, 8, 16, 10, 0);

        var result = ShiftMatchingHelper.DetectBestShift(shifts, scanTime);

        Assert.NotNull(result);
        Assert.Equal(_eveningShift.Id, result.Id);
    }

    [Fact]
    public void DetectBestShift_ShouldReturnNightShift_WhenScanTimeCrossesMidnight()
    {
        var shifts = new List<Shift> { _morningShift, _eveningShift, _nightShift };
        // 23:05 is around night shift start (23:00)
        var scanTime = new DateTime(2026, 8, 8, 23, 5, 0);

        var result = ShiftMatchingHelper.DetectBestShift(shifts, scanTime);

        Assert.NotNull(result);
        Assert.Equal(_nightShift.Id, result.Id);
    }

    [Fact]
    public void DetectBestShift_ShouldReturnNull_WhenShiftsListIsEmpty()
    {
        var result = ShiftMatchingHelper.DetectBestShift(new List<Shift>(), DateTime.Now);

        Assert.Null(result);
    }
}
