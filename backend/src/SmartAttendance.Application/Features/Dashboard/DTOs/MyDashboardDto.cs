using System;
using System.Collections.Generic;

namespace SmartAttendance.Application.Features.Dashboard.DTOs;

/// <summary>
/// Simplified dashboard for a single employee (self-service view).
/// </summary>
public class MyDashboardDto
{
    // Today's status
    public string TodayStatus { get; set; } = "NotCheckedIn"; // NotCheckedIn | Present | Late | Absent | OnLeave
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }

    // Summary for this month
    public int TotalWorkingDaysThisMonth { get; set; }
    public int PresentDaysThisMonth { get; set; }
    public int LateDaysThisMonth { get; set; }
    public int AbsentDaysThisMonth { get; set; }
    public double AttendanceRateThisMonth { get; set; }

    // Leave balance
    public int TotalLeavesTaken { get; set; }
    public int PendingLeaveRequests { get; set; }

    // Last 7 days attendance
    public List<MyAttendanceDayDto> Last7Days { get; set; } = new();

    // Upcoming leaves
    public List<UpcomingLeaveDto> UpcomingLeaves { get; set; } = new();

    // Notifications
    public List<DashboardNotificationDto> Notifications { get; set; } = new();
}

public class MyAttendanceDayDto
{
    public DateOnly Date { get; set; }
    public string Status { get; set; } = "Absent"; // Present | Late | Absent | OnLeave | Weekend
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
}
