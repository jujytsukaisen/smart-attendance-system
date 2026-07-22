using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Leave : BaseTenantEntity
{
    // Identity
    public Guid EmployeeId { get; private set; }

    // Leave Information
    public LeaveType LeaveType { get; private set; }
    public LeaveStatus LeaveStatus { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Reason { get; private set; }
    public string? Remarks { get; private set; }

    public DateTime RequestedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public Guid? ApprovedBy { get; private set; }
    public Guid? RejectedBy { get; private set; }
    public Guid? CancelledBy { get; private set; }

    // Calculated
    public double TotalDays { get; private set; }

    // Relationships
    public Employee Employee { get; private set; } = null!;

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Leave() { }
#pragma warning restore CS8618

    private Leave(
        Guid companyId,
        Guid employeeId,
        LeaveType leaveType,
        DateTime startDate,
        DateTime endDate,
        string reason)
    {
        CompanyId = companyId;
        EmployeeId = employeeId;
        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        Reason = reason;

        LeaveStatus = LeaveStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        TotalDays = CalculateTotalDays(startDate, endDate);
    }

    public static Leave Request(
        Guid companyId,
        Guid employeeId,
        LeaveType leaveType,
        DateTime startDate,
        DateTime endDate,
        string reason)
    {
        if (companyId == Guid.Empty)
            throw new InvalidLeaveException("Company ID is required.");
        if (employeeId == Guid.Empty)
            throw new InvalidLeaveException("Employee ID is required.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidLeaveException("Leave reason is required.");

        if (startDate > endDate)
            throw new InvalidLeaveException("Start date cannot be after end date.");

        var totalDays = CalculateTotalDays(startDate, endDate);
        if (totalDays <= 0)
            throw new InvalidLeaveException("Leave duration must be greater than zero.");

        return new Leave(
            companyId,
            employeeId,
            leaveType,
            startDate,
            endDate,
            reason);
    }

    public void Approve(Guid approvedBy, string? remarks = null)
    {
        if (LeaveStatus == LeaveStatus.Approved) return;
        
        if (LeaveStatus == LeaveStatus.Rejected)
            throw new InvalidLeaveException("Rejected leave cannot be approved.");
            
        if (LeaveStatus == LeaveStatus.Cancelled)
            throw new InvalidLeaveException("Cancelled leave cannot be approved.");

        if (approvedBy == Guid.Empty)
            throw new InvalidLeaveException("ApprovedBy ID is required.");

        LeaveStatus = LeaveStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        
        if (!string.IsNullOrWhiteSpace(remarks))
        {
            Remarks = remarks;
        }
    }

    public void Reject(Guid rejectedBy, string? remarks = null)
    {
        if (LeaveStatus == LeaveStatus.Rejected) return;

        if (LeaveStatus == LeaveStatus.Approved)
            throw new InvalidLeaveException("Approved leave cannot be rejected.");

        if (rejectedBy == Guid.Empty)
            throw new InvalidLeaveException("RejectedBy ID is required.");

        LeaveStatus = LeaveStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectedBy = rejectedBy;
        
        if (!string.IsNullOrWhiteSpace(remarks))
        {
            Remarks = remarks;
        }
    }

    public void Cancel(Guid cancelledBy, string? remarks = null)
    {
        if (LeaveStatus == LeaveStatus.Cancelled) return;

        LeaveStatus = LeaveStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelledBy = cancelledBy;
        
        if (!string.IsNullOrWhiteSpace(remarks))
        {
            Remarks = remarks;
        }
    }

    public void UpdateReason(string reason)
    {
        if (LeaveStatus == LeaveStatus.Approved)
            throw new InvalidLeaveException("Approved leave cannot be modified.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidLeaveException("Leave reason cannot be empty.");

        Reason = reason;
    }

    private static double CalculateTotalDays(DateTime startDate, DateTime endDate)
    {
        // Simple calculation mapping dates inclusive of start and end
        return (endDate.Date - startDate.Date).TotalDays + 1;
    }
}

