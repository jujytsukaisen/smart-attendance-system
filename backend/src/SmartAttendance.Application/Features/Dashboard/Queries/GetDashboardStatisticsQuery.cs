using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Dashboard.DTOs;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Dashboard.Queries;

public class GetDashboardStatisticsQuery : IRequest<Result<DashboardStatisticsDto>>
{
}

public class GetDashboardStatisticsQueryHandler
    : IRequestHandler<GetDashboardStatisticsQuery, Result<DashboardStatisticsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardStatisticsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardStatisticsDto>> Handle(
        GetDashboardStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // DateTime equivalents for comparing with DateTime fields (StartDate/EndDate are DateTime)
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1).AddTicks(-1);

        // Employee.EmployeeStatus == EmployeeStatus.Active  (لا يوجد IsActive)
        var totalEmployees = await _context.Employees
            .Where(e => e.EmployeeStatus == EmployeeStatus.Active)
            .CountAsync(cancellationToken);

        // Attendance لا تحتوي على Date، نستخدم DateTime range للمقارنة مع CheckInTime
        var todayAttendances = await _context.Attendances
            .Where(a => a.CheckInTime >= todayStart && a.CheckInTime <= todayEnd)
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .ToListAsync(cancellationToken);

        // Group attendances by employee and get the latest one for today
        var latestAttendancesToday = todayAttendances
            .GroupBy(a => a.EmployeeId)
            .Select(g => g.OrderByDescending(a => a.CheckInTime).First())
            .ToList();

        var presentToday = latestAttendancesToday.Count(a => a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave);
        var lateToday    = latestAttendancesToday.Count(a => a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave);
        var absentToday  = Math.Max(0, totalEmployees - latestAttendancesToday.Count);

        // Leave.LeaveStatus وليس Status، وStartDate/EndDate هي DateTime لذا نقارن بـ DateTime
        var employeesOnLeave = await _context.Leaves
            .Where(l => l.LeaveStatus == LeaveStatus.Approved
                        && l.StartDate <= todayEnd && l.EndDate >= todayStart)
            .CountAsync(cancellationToken);

        var activeDevices = await _context.AttendanceDevices
            .Where(d => d.IsActive)
            .CountAsync(cancellationToken);

        var attendanceRate = totalEmployees > 0
            ? Math.Round((double)(presentToday + lateToday) / totalEmployees * 100, 1)
            : 0;

        // ── OnBreakToday: employees whose latest event today is BreakStart ──
        var todayEventGroups = await _context.AttendanceEvents
            .Where(e => e.Timestamp >= todayStart && e.Timestamp <= todayEnd)
            .GroupBy(e => e.EmployeeId)
            .ToListAsync(cancellationToken);

        var onBreakToday = todayEventGroups.Count(g =>
        {
            var latestEvent = g.OrderByDescending(e => e.Timestamp).FirstOrDefault();
            return latestEvent?.EventType == AttendanceEventType.BreakStart;
        });

        // آخر 7 أيام - نستخدم DateOnly للمقارنة مع CheckInTime
        var sevenDaysAgo        = today.AddDays(-6);
        var sevenDaysAgoDateTime = todayStart.AddDays(-6);

        var last7DaysAttendances = await _context.Attendances
            .Where(a => a.CheckInTime >= sevenDaysAgoDateTime && a.CheckInTime <= todayEnd)
            .ToListAsync(cancellationToken);

        // ⚠️ نجلب تواريخ تعيين الموظفين النشطين لحساب "الغياب" بدقة
        // بدل افتراض إن كل الموظفين الحاليين كانوا موجودين طوال الأسبوع الماضي
        var activeEmployeeHireDates = await _context.Employees
            .Where(e => e.EmployeeStatus == EmployeeStatus.Active)
            .Select(e => e.HireDate)
            .ToListAsync(cancellationToken);

        var attendanceLast7Days = Enumerable.Range(0, 7)
            .Select(offset => sevenDaysAgo.AddDays(offset))
            .Select(date =>
            {
                var employeesEmployedThatDay = activeEmployeeHireDates.Count(h => DateOnly.FromDateTime(h) <= date);
                
                var dayAttendances = last7DaysAttendances
                    .Where(a => DateOnly.FromDateTime(a.CheckInTime) == date)
                    .GroupBy(a => a.EmployeeId)
                    .Select(g => g.OrderByDescending(a => a.CheckInTime).First())
                    .ToList();

                var attendedThatDay = dayAttendances.Count;

                return new AttendanceDayDto
                {
                    Date    = date,
                    Present = dayAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave),
                    Late    = dayAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave),
                    Absent  = Math.Max(0, employeesEmployedThatDay - attendedThatDay)
                };
            })
            .ToList();

        // توزيع الموظفين حسب الأقسام - EmployeeStatus == Active وليس IsActive
        var departmentStatistics = await _context.Departments
            .Where(d => d.Branch != null && !d.Branch.IsDeleted)
            .Select(d => new DepartmentStatisticDto
            {
                Department = d.Name,
                Count      = d.Employees.Count(e => e.EmployeeStatus == EmployeeStatus.Active)
            })
            .ToListAsync(cancellationToken);

        // آخر عمليات الحضور
        var recentAttendance = todayAttendances
            .OrderByDescending(a => a.CheckInTime)
            .Take(10)
            .Select(a => new RecentAttendanceDto
            {
                Id           = a.Id,
                EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
                Department   = a.Employee.Department?.Name ?? string.Empty,
                CheckIn      = a.CheckInTime,
                CheckOut     = a.CheckOutTime,
                Status       = a.AttendanceStatus.ToString()
            })
            .ToList();

        // أقرب الإجازات - نجلب السجلات ثم نحول DateOnly في الذاكرة لتفادي مشاكل EF Core Translation
        var upcomingLeavesEntities = await _context.Leaves
            .Where(l => l.StartDate >= todayStart)
            .OrderBy(l => l.StartDate)
            .Take(10)
            .Include(l => l.Employee)
            .ToListAsync(cancellationToken);

        var upcomingLeaves = upcomingLeavesEntities
            .Select(l => new UpcomingLeaveDto
            {
                Id           = l.Id,
                EmployeeName = $"{l.Employee?.FirstName} {l.Employee?.LastName}".Trim(),
                LeaveType    = l.LeaveType.ToString(),
                From         = DateOnly.FromDateTime(l.StartDate),
                To           = DateOnly.FromDateTime(l.EndDate),
                Status       = l.LeaveStatus.ToString()
            })
            .ToList();

        // آخر التنبيهات الخاصة بالمستخدم الحالي
        Guid.TryParse(_currentUserService.UserId, out var currentUserId);
        var notifications = await _context.Notifications
            .Where(n => n.RecipientUserId == currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new DashboardNotificationDto
            {
                Id        = n.Id,
                Message   = n.Message,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new DashboardStatisticsDto
        {
            TotalEmployees      = totalEmployees,
            PresentToday        = presentToday,
            AbsentToday         = absentToday,
            LateToday           = lateToday,
            OnBreakToday        = onBreakToday,
            EmployeesOnLeave    = employeesOnLeave,
            ActiveDevices       = activeDevices,
            AttendanceRate      = attendanceRate,
            AttendanceLast7Days = attendanceLast7Days,
            DepartmentStatistics = departmentStatistics,
            RecentAttendance    = recentAttendance,
            UpcomingLeaves      = upcomingLeaves,
            Notifications       = notifications
        };

        return Result<DashboardStatisticsDto>.Success(result);
    }
}