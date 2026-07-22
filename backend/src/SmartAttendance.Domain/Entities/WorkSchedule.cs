using System;
using System.Collections.Generic;
using System.Linq;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class WorkSchedule : BaseTenantEntity
{
    private readonly List<Shift> _shifts = new();
    private readonly List<Employee> _employees = new();

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsFlexible { get; private set; }
    public bool IsActive { get; private set; }
    public double WeeklyWorkingHours { get; private set; }
    public int GracePeriodMinutes { get; private set; }
    public int AllowEarlyCheckInMinutes { get; private set; }
    public int AllowLateCheckOutMinutes { get; private set; }

    public IReadOnlyCollection<Shift> Shifts => _shifts.AsReadOnly();
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private WorkSchedule() { }
#pragma warning restore CS8618

    private WorkSchedule(
        Guid companyId,
        string name,
        string? description,
        bool isFlexible,
        double weeklyWorkingHours,
        int gracePeriodMinutes,
        int allowEarlyCheckInMinutes,
        int allowLateCheckOutMinutes)
    {
        CompanyId = companyId;
        Name = name;
        Description = description;
        IsFlexible = isFlexible;
        WeeklyWorkingHours = weeklyWorkingHours;
        GracePeriodMinutes = gracePeriodMinutes;
        AllowEarlyCheckInMinutes = allowEarlyCheckInMinutes;
        AllowLateCheckOutMinutes = allowLateCheckOutMinutes;
        IsDefault = false;
        IsActive = false; // Intentionally inactive until a shift is added
    }

    public static WorkSchedule Create(
        Guid companyId,
        string name,
        string? description,
        bool isFlexible,
        double weeklyWorkingHours,
        int gracePeriodMinutes,
        int allowEarlyCheckInMinutes,
        int allowLateCheckOutMinutes)
    {
        if (companyId == Guid.Empty)
            throw new InvalidWorkScheduleException("Company ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidWorkScheduleException("Work schedule name cannot be empty.");
        if (weeklyWorkingHours <= 0)
            throw new InvalidWorkScheduleException("Weekly working hours must be greater than zero.");
        if (gracePeriodMinutes < 0)
            throw new InvalidWorkScheduleException("Grace period minutes cannot be negative.");
        if (allowEarlyCheckInMinutes < 0)
            throw new InvalidWorkScheduleException("Allow early check-in minutes cannot be negative.");
        if (allowLateCheckOutMinutes < 0)
            throw new InvalidWorkScheduleException("Allow late check-out minutes cannot be negative.");

        return new WorkSchedule(
            companyId,
            name,
            description,
            isFlexible,
            weeklyWorkingHours,
            gracePeriodMinutes,
            allowEarlyCheckInMinutes,
            allowLateCheckOutMinutes);
    }

    public void Update(
        string name,
        string? description,
        bool isFlexible,
        double weeklyWorkingHours,
        int gracePeriodMinutes,
        int allowEarlyCheckInMinutes,
        int allowLateCheckOutMinutes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidWorkScheduleException("Work schedule name cannot be empty.");
        if (weeklyWorkingHours <= 0)
            throw new InvalidWorkScheduleException("Weekly working hours must be greater than zero.");
        if (gracePeriodMinutes < 0)
            throw new InvalidWorkScheduleException("Grace period minutes cannot be negative.");
        if (allowEarlyCheckInMinutes < 0)
            throw new InvalidWorkScheduleException("Allow early check-in minutes cannot be negative.");
        if (allowLateCheckOutMinutes < 0)
            throw new InvalidWorkScheduleException("Allow late check-out minutes cannot be negative.");

        Name = name;
        Description = description;
        IsFlexible = isFlexible;
        WeeklyWorkingHours = weeklyWorkingHours;
        GracePeriodMinutes = gracePeriodMinutes;
        AllowEarlyCheckInMinutes = allowEarlyCheckInMinutes;
        AllowLateCheckOutMinutes = allowLateCheckOutMinutes;
    }

    public void Activate()
    {
        if (IsActive) return;

        if (_shifts.Count == 0)
            throw new InvalidWorkScheduleException("A schedule must contain at least one shift before activation.");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void MarkAsDefault()
    {
        if (IsDefault) return;
        IsDefault = true;
    }

    public void RemoveDefault()
    {
        if (!IsDefault) return;
        IsDefault = false;
    }

    public void AddShift(Shift shift)
    {
        if (shift == null)
            throw new InvalidWorkScheduleException("Shift cannot be null.");

        if (_shifts.Any(s => s.Id == shift.Id))
            throw new InvalidWorkScheduleException("Shift is already added to this schedule.");

        _shifts.Add(shift);
    }

    public void RemoveShift(Shift shift)
    {
        if (shift == null)
            throw new InvalidWorkScheduleException("Shift cannot be null.");

        var existingShift = _shifts.FirstOrDefault(s => s.Id == shift.Id);
        if (existingShift == null)
            return;

        if (IsActive && _shifts.Count == 1)
            throw new InvalidWorkScheduleException("Cannot remove the last shift from an active schedule.");

        _shifts.Remove(existingShift);
    }

    public void ReplaceShifts(IEnumerable<Shift> shifts)
    {
        if (shifts == null)
            throw new InvalidWorkScheduleException("Shifts collection cannot be null.");

        var shiftsList = shifts.ToList();
        
        if (IsActive && shiftsList.Count == 0)
            throw new InvalidWorkScheduleException("Cannot replace shifts with an empty collection for an active schedule.");

        _shifts.Clear();

        foreach (var shift in shiftsList)
        {
            if (_shifts.Any(s => s.Id == shift.Id))
                throw new InvalidWorkScheduleException("Shift is duplicated in the replacement collection.");

            _shifts.Add(shift);
        }
    }

    public void AssignEmployee(Employee employee)
    {
        if (employee == null)
            throw new InvalidWorkScheduleException("Employee cannot be null.");

        if (!IsActive)
            throw new InvalidWorkScheduleException("An inactive schedule cannot be assigned to employees.");

        if (_employees.Any(e => e.Id == employee.Id))
            throw new InvalidWorkScheduleException("Employee is already assigned to this schedule.");

        _employees.Add(employee);
    }

    public void RemoveEmployee(Employee employee)
    {
        if (employee == null)
            throw new InvalidWorkScheduleException("Employee cannot be null.");

        var existingEmployee = _employees.FirstOrDefault(e => e.Id == employee.Id);
        if (existingEmployee == null)
            return;

        _employees.Remove(existingEmployee);
    }

    public void Delete()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        IsActive = false; // Cannot be active if deleted
        IsDefault = false; // Cannot be default if deleted
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        
        IsDeleted = false;
    }
}

