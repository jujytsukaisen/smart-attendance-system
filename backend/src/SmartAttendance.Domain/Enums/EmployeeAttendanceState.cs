namespace SmartAttendance.Domain.Enums;

/// <summary>
/// The computed real-time state of an employee for the current workday,
/// derived from the ordered sequence of <see cref="AttendanceEventType"/> events.
/// This value is NOT persisted — it is always derived on-the-fly from AttendanceEvents.
/// </summary>
public enum EmployeeAttendanceState
{
    /// <summary>No attendance record exists for today yet.</summary>
    NotCheckedIn = 0,

    /// <summary>The employee has checked in and is currently working (not on break).</summary>
    Working = 1,

    /// <summary>The employee started a break and has not yet returned.</summary>
    OnBreak = 2,

    /// <summary>The employee has explicitly checked out. The day is closed — no re-entry allowed.</summary>
    Finished = 3
}
