using System;
using System.Collections.Generic;

namespace SmartAttendance.Application.Features.Dashboard.DTOs;

public class DashboardStatisticsDto
{
    public int TotalEmployees { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int LateToday { get; set; }
    /// <summary>Number of employees currently on a break (state = OnBreak) as of the query time.</summary>
    public int OnBreakToday { get; set; }
    public int EmployeesOnLeave { get; set; }
    public int ActiveDevices { get; set; }
    public double AttendanceRate { get; set; }

    public List<AttendanceDayDto> AttendanceLast7Days { get; set; } = new();
    public List<DepartmentStatisticDto> DepartmentStatistics { get; set; } = new();
    public List<RecentAttendanceDto> RecentAttendance { get; set; } = new();
    public List<UpcomingLeaveDto> UpcomingLeaves { get; set; } = new();
    public List<DashboardNotificationDto> Notifications { get; set; } = new();
}

public class AttendanceDayDto
{
    public DateOnly Date { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
}

public class DepartmentStatisticDto
{
    public string Department { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentAttendanceDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string Status { get; set; } = string.Empty; // Present / Late / Absent
}

public class UpcomingLeaveDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DashboardNotificationDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}