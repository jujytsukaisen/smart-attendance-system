using System;
using System.Collections.Generic;

namespace SmartAttendance.Application.Features.Reports.DTOs;

public class AttendanceTrendDataDto
{
    public DateOnly Date { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int OnLeave { get; set; }
}

public class NameValueDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class DepartmentAttendanceDto
{
    public string Name { get; set; } = string.Empty;
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
}

public class ReportsDashboardStatisticsDto
{
    public int TotalEmployees { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int LateToday { get; set; }
    public int OnLeaveToday { get; set; }
    public double AverageAttendanceRate { get; set; }
    public List<AttendanceTrendDataDto> WeeklyTrend { get; set; } = new();
    public List<NameValueDto> LeaveDistribution { get; set; } = new();
    public List<DepartmentAttendanceDto> DepartmentAttendance { get; set; } = new();
}

public class LateEmployeesReportDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int TotalLateMinutes { get; set; }
    public int LateDaysCount { get; set; }
}

public class AbsenceReportDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int AbsentDaysCount { get; set; }
}

public class LeaveReportDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public double TotalDays { get; set; }
}

public class ReportFilterDto
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
}