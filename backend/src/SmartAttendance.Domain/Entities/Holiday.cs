using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Holiday : BaseTenantEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public HolidayType HolidayType { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsRecurring { get; private set; }
    public bool IsActive { get; private set; }
    
    public Guid? BranchId { get; private set; }
    public Branch? Branch { get; private set; }

#pragma warning disable CS8618
    private Holiday() { }
#pragma warning restore CS8618

    private Holiday(
        Guid companyId,
        string name,
        string? description,
        HolidayType holidayType,
        DateTime startDate,
        DateTime endDate,
        bool isRecurring,
        Guid? branchId)
    {
        CompanyId = companyId;
        Name = name;
        Description = description;
        HolidayType = holidayType;
        StartDate = startDate;
        EndDate = endDate;
        IsRecurring = isRecurring;
        BranchId = branchId;
        
        IsActive = true;
    }

    public static Holiday Create(
        Guid companyId,
        string name,
        string? description,
        HolidayType holidayType,
        DateTime startDate,
        DateTime endDate,
        bool isRecurring,
        Guid? branchId = null)
    {
        if (companyId == Guid.Empty)
            throw new InvalidHolidayException("Company ID is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidHolidayException("Holiday name is required.");
        
        if (startDate > endDate)
            throw new InvalidHolidayException("Start date cannot be after end date.");

        if (isRecurring && (endDate - startDate).TotalDays > 365)
            throw new InvalidHolidayException("Recurring holiday range is invalid (spans more than a year).");

        return new Holiday(
            companyId,
            name,
            description,
            holidayType,
            startDate,
            endDate,
            isRecurring,
            branchId);
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

    public void Rename(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidHolidayException("Holiday name is required.");
            
        Name = name;
        
        if (description != null)
        {
            Description = description;
        }
    }

    public void ChangeDates(DateTime startDate, DateTime endDate, bool? isRecurring = null)
    {
        if (startDate > endDate)
            throw new InvalidHolidayException("Start date cannot be after end date.");

        var updatedRecurring = isRecurring ?? IsRecurring;

        if (updatedRecurring && (endDate - startDate).TotalDays > 365)
            throw new InvalidHolidayException("Recurring holiday range is invalid (spans more than a year).");

        StartDate = startDate;
        EndDate = endDate;
        IsRecurring = updatedRecurring;
    }
}

