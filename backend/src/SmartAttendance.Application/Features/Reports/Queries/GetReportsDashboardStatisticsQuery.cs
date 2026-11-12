using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Reports.DTOs;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Reports.Queries;

public class GetReportsDashboardStatisticsQuery : IRequest<Result<ReportsDashboardStatisticsDto>>
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class GetReportsDashboardStatisticsQueryHandler
    : IRequestHandler<GetReportsDashboardStatisticsQuery, Result<ReportsDashboardStatisticsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReportsDashboardStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReportsDashboardStatisticsDto>> Handle(
        GetReportsDashboardStatisticsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1).AddTicks(-1);
        var sevenDaysAgo = today.AddDays(-6);

        var totalEmployees = await _context.Employees
            .Where(e => e.EmployeeStatus == EmployeeStatus.Active)
            .CountAsync(cancellationToken);

        var todayAttendances = await _context.Attendances
            .Where(a => a.CheckInTime >= todayStart && a.CheckInTime <= todayEnd)
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .ToListAsync(cancellationToken);

        var presentToday = todayAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave);
        var lateToday = todayAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave);
        var absentToday = totalEmployees - todayAttendances.Count;

        var onLeaveToday = await _context.Leaves
            .Where(l => l.LeaveStatus == LeaveStatus.Approved
                        && l.StartDate <= DateTime.UtcNow && l.EndDate >= DateTime.UtcNow)
            .CountAsync(cancellationToken);

        var averageAttendanceRate = totalEmployees > 0
            ? Math.Round((double)(presentToday + lateToday) / totalEmployees * 100, 1)
            : 0;

        var last7DaysAttendances = await _context.Attendances
            .Where(a => a.CheckInTime >= sevenDaysAgo.ToDateTime(TimeOnly.MinValue))
            .ToListAsync(cancellationToken);

        var weeklyTrend = Enumerable.Range(0, 7)
            .Select(offset => sevenDaysAgo.AddDays(offset))
            .Select(date => new AttendanceTrendDataDto
            {
                Date = date,
                Present = last7DaysAttendances.Count(a => DateOnly.FromDateTime(a.CheckInTime) == date && (a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave)),
                Late = last7DaysAttendances.Count(a => DateOnly.FromDateTime(a.CheckInTime) == date && (a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave)),
                Absent = totalEmployees - last7DaysAttendances.Count(a => DateOnly.FromDateTime(a.CheckInTime) == date),
                OnLeave = 0
            })
            .ToList();

        var leaveDistribution = await _context.Leaves
            .Where(l => l.LeaveStatus == LeaveStatus.Approved)
            .GroupBy(l => l.LeaveType)
            .Select(g => new NameValueDto { Name = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        var departmentsWithEmployees = await _context.Departments
            .Include(d => d.Employees)
            .ToListAsync(cancellationToken);

        var departmentAttendance = departmentsWithEmployees
            .Select(d => new DepartmentAttendanceDto
            {
                Name = d.Name,
                Present = todayAttendances.Count(a => a.Employee.DepartmentId == d.Id && (a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave)),
                Late = todayAttendances.Count(a => a.Employee.DepartmentId == d.Id && (a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave)),
                Absent = d.Employees.Count(e => e.EmployeeStatus == EmployeeStatus.Active)
                         - todayAttendances.Count(a => a.Employee.DepartmentId == d.Id)
            })
            .ToList();

        var dto = new ReportsDashboardStatisticsDto
        {
            TotalEmployees = totalEmployees,
            PresentToday = presentToday,
            AbsentToday = absentToday,
            LateToday = lateToday,
            OnLeaveToday = onLeaveToday,
            AverageAttendanceRate = averageAttendanceRate,
            WeeklyTrend = weeklyTrend,
            LeaveDistribution = leaveDistribution,
            DepartmentAttendance = departmentAttendance
        };

        return Result<ReportsDashboardStatisticsDto>.Success(dto);
    }
}