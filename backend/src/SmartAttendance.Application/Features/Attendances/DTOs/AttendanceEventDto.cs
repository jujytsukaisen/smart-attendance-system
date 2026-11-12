using System;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.DTOs;

/// <summary>
/// Represents a single scan event within an attendance record's daily timeline.
/// Used to build the visual event timeline in the UI:
/// e.g. 08:00 CheckIn → 10:15 BreakStart → 10:30 BreakEnd → 16:00 CheckOut.
/// </summary>
public class AttendanceEventDto
{
    /// <summary>Unique identifier of this event.</summary>
    public Guid Id { get; set; }

    /// <summary>Parent attendance record identifier.</summary>
    public Guid AttendanceId { get; set; }

    /// <summary>UTC timestamp of the scan.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>The state-machine action this scan represents.</summary>
    public AttendanceEventType EventType { get; set; }

    /// <summary>Human-readable label for <see cref="EventType"/>.</summary>
    public string EventTypeLabel => EventType.ToString();

    /// <summary>Optional justification note attached to the event.</summary>
    public string? Note { get; set; }
}
