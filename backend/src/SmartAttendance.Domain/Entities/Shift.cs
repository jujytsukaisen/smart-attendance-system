using System;
using System.Collections.Generic;
using System.Linq;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Shift : BaseTenantEntity
{
    private readonly List<Break> _breaks = new();

    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public ShiftType ShiftType { get; private set; }
    public bool IsNightShift { get; private set; }
    public bool IsFlexible { get; private set; }
    public double MinimumWorkingHours { get; private set; }
    public double MaximumWorkingHours { get; private set; }
    public int GracePeriodMinutes { get; private set; }
    public int AllowEarlyCheckInMinutes { get; private set; }
    public int AllowLateCheckOutMinutes { get; private set; }
    public bool IsActive { get; private set; }

    public Guid WorkScheduleId { get; private set; }
    public WorkSchedule WorkSchedule { get; private set; } = null!;
    public IReadOnlyCollection<Break> Breaks => _breaks.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Shift() { }
#pragma warning restore CS8618

    private Shift(
        Guid companyId,
        Guid workScheduleId,
        string name,
        string code,
        string? description,
        TimeOnly startTime,
        TimeOnly endTime,
        ShiftType shiftType,
        bool isFlexible,
        double minimumWorkingHours,
        double maximumWorkingHours,
        int gracePeriodMinutes,
        int allowEarlyCheckInMinutes,
        int allowLateCheckOutMinutes)
    {
        CompanyId = companyId;
        WorkScheduleId = workScheduleId;
        Name = name;
        Code = code;
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        ShiftType = shiftType;
        IsFlexible = isFlexible;
        MinimumWorkingHours = minimumWorkingHours;
        MaximumWorkingHours = maximumWorkingHours;
        GracePeriodMinutes = gracePeriodMinutes;
        AllowEarlyCheckInMinutes = allowEarlyCheckInMinutes;
        AllowLateCheckOutMinutes = allowLateCheckOutMinutes;
        
        IsNightShift = EndTime < StartTime;
        IsActive = false;
    }

    public static Shift Create(
        Guid companyId,
        Guid workScheduleId,
        string name,
        string code,
        string? description,
        TimeOnly startTime,
        TimeOnly endTime,
        ShiftType shiftType,
        bool isFlexible,
        double minimumWorkingHours,
        double maximumWorkingHours,
        int gracePeriodMinutes,
        int allowEarlyCheckInMinutes,
        int allowLateCheckOutMinutes)
    {
        if (companyId == Guid.Empty)
            throw new InvalidShiftException("Company ID cannot be empty.");
        if (workScheduleId == Guid.Empty)
            throw new InvalidShiftException("Work schedule ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidShiftException("Shift name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidShiftException("Shift code cannot be empty.");
        if (minimumWorkingHours <= 0)
            throw new InvalidShiftException("Minimum working hours must be greater than zero.");
        if (maximumWorkingHours < minimumWorkingHours)
            throw new InvalidShiftException("Maximum working hours must be greater than or equal to minimum working hours.");
        if (gracePeriodMinutes < 0)
            throw new InvalidShiftException("Grace period minutes cannot be negative.");
        if (allowEarlyCheckInMinutes < 0)
            throw new InvalidShiftException("Allow early check-in minutes cannot be negative.");
        if (allowLateCheckOutMinutes < 0)
            throw new InvalidShiftException("Allow late check-out minutes cannot be negative.");

        return new Shift(
            companyId,
            workScheduleId,
            name,
            code,
            description,
            startTime,
            endTime,
            shiftType,
            isFlexible,
            minimumWorkingHours,
            maximumWorkingHours,
            gracePeriodMinutes,
            allowEarlyCheckInMinutes,
            allowLateCheckOutMinutes);
    }

    public void Update(string name, string code, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidShiftException("Shift name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidShiftException("Shift code cannot be empty.");

        Name = name;
        Code = code;
        Description = description;
    }

    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
    }

    public void ChangeWorkingHours(TimeOnly startTime, TimeOnly endTime, double minimumWorkingHours, double maximumWorkingHours)
    {
        if (minimumWorkingHours <= 0)
            throw new InvalidShiftException("Minimum working hours must be greater than zero.");
        if (maximumWorkingHours < minimumWorkingHours)
            throw new InvalidShiftException("Maximum working hours must be greater than or equal to minimum working hours.");

        StartTime = startTime;
        EndTime = endTime;
        MinimumWorkingHours = minimumWorkingHours;
        MaximumWorkingHours = maximumWorkingHours;
        
        // Dynamically compute overnight state
        IsNightShift = EndTime < StartTime;
    }

    public void ChangeGracePeriod(int gracePeriodMinutes)
    {
        if (gracePeriodMinutes < 0)
            throw new InvalidShiftException("Grace period minutes cannot be negative.");

        GracePeriodMinutes = gracePeriodMinutes;
    }

    public void ChangeCheckInRules(int allowEarlyCheckInMinutes, int allowLateCheckOutMinutes)
    {
        if (allowEarlyCheckInMinutes < 0)
            throw new InvalidShiftException("Allow early check-in minutes cannot be negative.");
        if (allowLateCheckOutMinutes < 0)
            throw new InvalidShiftException("Allow late check-out minutes cannot be negative.");

        AllowEarlyCheckInMinutes = allowEarlyCheckInMinutes;
        AllowLateCheckOutMinutes = allowLateCheckOutMinutes;
    }

    public void AddBreak(Break @break)
    {
        if (@break == null)
            throw new InvalidShiftException("Break cannot be null.");

        if (_breaks.Any(b => b.Id == @break.Id))
            throw new InvalidShiftException("This break is already added to the shift.");

        if (_breaks.Any(b => b.StartTime < @break.EndTime && @break.StartTime < b.EndTime))
            throw new InvalidShiftException("Breaks cannot overlap.");

        _breaks.Add(@break);
    }

    public void RemoveBreak(Break @break)
    {
        if (@break == null)
            throw new InvalidShiftException("Break cannot be null.");

        var existingBreak = _breaks.FirstOrDefault(b => b.Id == @break.Id);
        if (existingBreak == null)
            return;

        _breaks.Remove(existingBreak);
    }

    public void ReplaceBreaks(IEnumerable<Break> breaks)
    {
        if (breaks == null)
            throw new InvalidShiftException("Breaks collection cannot be null.");

        var breakList = breaks.ToList();

        _breaks.Clear();

        foreach (var @break in breakList)
        {
            if (_breaks.Any(b => b.Id == @break.Id))
                throw new InvalidShiftException("Duplicate break detected in the replacement collection.");

            _breaks.Add(@break);
        }
    }
}

