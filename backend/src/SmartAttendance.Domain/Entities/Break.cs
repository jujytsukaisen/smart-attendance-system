using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Break : BaseTenantEntity
{
    // Identity
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    // Time
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int DurationMinutes { get; private set; }
    
    // Configuration
    public BreakType BreakType { get; private set; }
    public bool IsPaid { get; private set; }
    public bool IsMandatory { get; private set; }
    public bool IsActive { get; private set; }
    
    // Relationships
    public Guid ShiftId { get; private set; }
    public Shift Shift { get; private set; } = null!;

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Break() { }
#pragma warning restore CS8618

    private Break(
        Guid companyId,
        Guid shiftId,
        string name,
        string? description,
        TimeOnly startTime,
        TimeOnly endTime,
        int durationMinutes,
        BreakType breakType,
        bool isPaid,
        bool isMandatory)
    {
        CompanyId = companyId;
        ShiftId = shiftId;
        Name = name;
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        DurationMinutes = durationMinutes;
        BreakType = breakType;
        IsPaid = isPaid;
        IsMandatory = isMandatory;
        
        IsActive = false; // By default, let's keep it consistent with other entities until activated
    }

    public static Break Create(
        Guid companyId,
        Guid shiftId,
        string name,
        string? description,
        TimeOnly startTime,
        TimeOnly endTime,
        int durationMinutes,
        BreakType breakType,
        bool isPaid,
        bool isMandatory)
    {
        if (companyId == Guid.Empty)
            throw new InvalidBreakException("Company ID cannot be empty.");
        if (shiftId == Guid.Empty)
            throw new InvalidBreakException("Shift ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidBreakException("Name cannot be empty.");
        if (startTime == endTime)
            throw new InvalidBreakException("Start time must not equal end time.");
        if (durationMinutes <= 0)
            throw new InvalidBreakException("Duration must be greater than zero.");

        return new Break(
            companyId,
            shiftId,
            name,
            description,
            startTime,
            endTime,
            durationMinutes,
            breakType,
            isPaid,
            isMandatory);
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

    public void ChangeTime(TimeOnly startTime, TimeOnly endTime, int durationMinutes)
    {
        if (startTime == endTime)
            throw new InvalidBreakException("Start time must not equal end time.");
        if (durationMinutes <= 0)
            throw new InvalidBreakException("Duration must be greater than zero.");

        // Note: Overnight breaks are allowed, thus no strict start < end comparison is enforced here.

        StartTime = startTime;
        EndTime = endTime;
        DurationMinutes = durationMinutes;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidBreakException("Name cannot be empty.");

        Name = name;
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
    }

    public void MarkAsUnpaid()
    {
        IsPaid = false;
    }

    public void MarkAsMandatory()
    {
        IsMandatory = true;
    }

    public void MarkAsOptional()
    {
        IsMandatory = false;
    }
}

