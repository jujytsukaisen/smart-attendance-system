namespace SmartAttendance.Domain.Enums;

/// <summary>
/// Represents the type of an attendance scan event within the daily state machine.
/// Every physical scan by an employee produces exactly one AttendanceEvent of one of these types.
/// </summary>
public enum AttendanceEventType
{
    /// <summary>First scan of the day — starts the attendance record.</summary>
    CheckIn = 1,

    /// <summary>Scan while in Working state — employee is leaving temporarily.</summary>
    BreakStart = 2,

    /// <summary>Scan while On Break state — employee is returning.</summary>
    BreakEnd = 3,

    /// <summary>Explicit end-of-shift scan — closes the attendance record for the day.</summary>
    CheckOut = 4
}
