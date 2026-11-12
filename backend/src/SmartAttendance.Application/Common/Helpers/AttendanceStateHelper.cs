using System.Collections.Generic;
using System.Linq;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Common.Helpers;

/// <summary>
/// Pure-function helpers for computing the derived <see cref="EmployeeAttendanceState"/>
/// from an ordered sequence of <see cref="AttendanceEvent"/> records.
/// This logic is stateless and has no side effects — safe to call from both
/// query handlers and CQRS command handlers.
/// </summary>
public static class AttendanceStateHelper
{
    /// <summary>
    /// Derives the current <see cref="EmployeeAttendanceState"/> from the event collection.
    /// </summary>
    /// <param name="events">All events belonging to one attendance record, in any order.</param>
    /// <returns>The current state based on the latest event type.</returns>
    public static EmployeeAttendanceState ComputeState(IEnumerable<AttendanceEvent> events)
    {
        return ComputeState(events, null);
    }

    /// <summary>
    /// Derives the current <see cref="EmployeeAttendanceState"/> from the event collection and optional check-out time.
    /// </summary>
    /// <param name="events">All events belonging to one attendance record, in any order.</param>
    /// <param name="checkOutTime">Optional final check-out timestamp if already recorded on parent attendance.</param>
    /// <returns>The current state based on the latest event type or check-out status.</returns>
    public static EmployeeAttendanceState ComputeState(IEnumerable<AttendanceEvent>? events, DateTime? checkOutTime)
    {
        if (checkOutTime.HasValue)
            return EmployeeAttendanceState.Finished;

        if (events == null)
            return EmployeeAttendanceState.NotCheckedIn;

        var ordered = events.OrderBy(e => e.Timestamp).ToList();

        if (ordered.Count == 0)
            return EmployeeAttendanceState.NotCheckedIn;

        var lastEvent = ordered[^1];

        return lastEvent.EventType switch
        {
            AttendanceEventType.CheckIn    => EmployeeAttendanceState.Working,
            AttendanceEventType.BreakStart => EmployeeAttendanceState.OnBreak,
            AttendanceEventType.BreakEnd   => EmployeeAttendanceState.Working,
            AttendanceEventType.CheckOut   => EmployeeAttendanceState.Finished,
            _                              => EmployeeAttendanceState.NotCheckedIn
        };
    }
}
