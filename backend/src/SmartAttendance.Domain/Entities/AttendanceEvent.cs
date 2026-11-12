using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

/// <summary>
/// Records a single scan/swipe event in the employee's daily attendance state machine.
/// Every physical scan (card swipe, face recognition, mobile tap, etc.) produces exactly
/// one <see cref="AttendanceEvent"/> row.  The ordered sequence of events for an
/// <see cref="Attendance"/> record drives the state machine:
/// <c>CheckIn → BreakStart ⇄ BreakEnd (repeatable) → CheckOut</c>.
/// </summary>
public class AttendanceEvent : BaseTenantEntity
{
    // ── Relationships ──────────────────────────────────────────────────────────

    /// <summary>The daily attendance summary record this event belongs to.</summary>
    public Guid AttendanceId { get; private set; }

    /// <summary>The employee who triggered this event.</summary>
    public Guid EmployeeId { get; private set; }

    // ── Core Data ──────────────────────────────────────────────────────────────

    /// <summary>UTC timestamp of the scan.</summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>The action this scan represents within the attendance state machine.</summary>
    public AttendanceEventType EventType { get; private set; }

    /// <summary>Optional free-text note (e.g. reason for early check-out, override justification).</summary>
    public string? Note { get; private set; }

    // ── Navigation ─────────────────────────────────────────────────────────────

    /// <summary>Navigation property to the parent daily attendance record.</summary>
    public Attendance Attendance { get; private set; } = null!;

    /// <summary>Navigation property to the employee.</summary>
    public Employee Employee { get; private set; } = null!;

    // ── Constructors ───────────────────────────────────────────────────────────

    /// <summary>Private constructor for EF Core materialization (persistence-agnostic).</summary>
#pragma warning disable CS8618
    private AttendanceEvent() { }
#pragma warning restore CS8618

    private AttendanceEvent(
        Guid companyId,
        Guid attendanceId,
        Guid employeeId,
        DateTime timestamp,
        AttendanceEventType eventType,
        string? note)
    {
        CompanyId    = companyId;
        AttendanceId = attendanceId;
        EmployeeId   = employeeId;
        Timestamp    = timestamp;
        EventType    = eventType;
        Note         = note;
    }

    // ── Factory Method ─────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a validated <see cref="AttendanceEvent"/>.
    /// </summary>
    /// <param name="companyId">Tenant identifier.</param>
    /// <param name="attendanceId">Parent <see cref="Attendance"/> record.</param>
    /// <param name="employeeId">The scanning employee.</param>
    /// <param name="timestamp">UTC time of the scan.</param>
    /// <param name="eventType">The state-machine action this scan represents.</param>
    /// <param name="note">Optional justification note.</param>
    /// <returns>A new <see cref="AttendanceEvent"/> instance.</returns>
    /// <exception cref="InvalidAttendanceException">Thrown when required identifiers are missing.</exception>
    public static AttendanceEvent Create(
        Guid companyId,
        Guid attendanceId,
        Guid employeeId,
        DateTime timestamp,
        AttendanceEventType eventType,
        string? note = null)
    {
        if (companyId == Guid.Empty)
            throw new InvalidAttendanceException("Company ID is required for an attendance event.");
        if (attendanceId == Guid.Empty)
            throw new InvalidAttendanceException("Attendance ID is required for an attendance event.");
        if (employeeId == Guid.Empty)
            throw new InvalidAttendanceException("Employee ID is required for an attendance event.");

        return new AttendanceEvent(companyId, attendanceId, employeeId, timestamp, eventType, note);
    }
}
