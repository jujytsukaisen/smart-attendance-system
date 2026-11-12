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

public class GetMyDashboardQuery : IRequest<Result<MyDashboardDto>>
{
}

public class GetMyDashboardQueryHandler : IRequestHandler<GetMyDashboardQuery, Result<MyDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyDashboardQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MyDashboardDto>> Handle(GetMyDashboardQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var domainUserId))
        {
            return Result<MyDashboardDto>.Failure(new[] { "User ID is required." });
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == domainUserId && !e.IsDeleted, cancellationToken);

        if (employee == null)
        {
            // Return a blank dashboard if user has no linked employee
            return Result<MyDashboardDto>.Success(new MyDashboardDto());
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);

        // ── Today's attendance ──
        var todayAttendance = await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id
                && a.CheckInTime >= todayStart && a.CheckInTime <= todayEnd
                && !a.IsDeleted, cancellationToken);

        string todayStatus = "NotCheckedIn";
        if (todayAttendance != null)
        {
            todayStatus = todayAttendance.AttendanceStatus switch
            {
                AttendanceStatus.Present or AttendanceStatus.PresentDuringLeave => "Present",
                AttendanceStatus.Late or AttendanceStatus.LateDuringLeave => "Late",
                _ => todayAttendance.AttendanceStatus.ToString()
            };
        }

        // Check if on leave today
        var onLeaveToday = await _context.Leaves
            .AsNoTracking()
            .AnyAsync(l => l.EmployeeId == employee.Id
                && l.LeaveStatus == LeaveStatus.Approved
                && l.StartDate <= todayEnd && l.EndDate >= todayStart
                && !l.IsDeleted, cancellationToken);

        if (todayAttendance == null && onLeaveToday)
            todayStatus = "OnLeave";

        // ── This month summary ──
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);
        var monthAttendances = await _context.Attendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employee.Id
                && a.CheckInTime >= monthStart && a.CheckInTime <= todayEnd
                && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        var workingDays = (int)(todayStart - monthStart).TotalDays + 1; // simplistic
        var presentDays = monthAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave);
        var lateDays = monthAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave);
        var attendedDays = monthAttendances.Count;
        var absentDays = Math.Max(0, workingDays - attendedDays);
        var attendanceRate = workingDays > 0 ? Math.Round((double)attendedDays / workingDays * 100, 1) : 0;

        // ── Leaves ──
        var totalLeavesTaken = await _context.Leaves
            .AsNoTracking()
            .CountAsync(l => l.EmployeeId == employee.Id
                && l.LeaveStatus == LeaveStatus.Approved
                && !l.IsDeleted, cancellationToken);

        var pendingLeaveRequests = await _context.Leaves
            .AsNoTracking()
            .CountAsync(l => l.EmployeeId == employee.Id
                && l.LeaveStatus == LeaveStatus.Pending
                && !l.IsDeleted, cancellationToken);

        // ── Last 7 days ──
        var sevenDaysAgo = todayStart.AddDays(-6);
        var last7Attendances = await _context.Attendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employee.Id
                && a.CheckInTime >= sevenDaysAgo && a.CheckInTime <= todayEnd
                && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        var last7Days = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(sevenDaysAgo.AddDays(offset));
                var att = last7Attendances.FirstOrDefault(a => DateOnly.FromDateTime(a.CheckInTime) == date);
                string status = "Absent";
                if (att != null)
                {
                    status = (att.AttendanceStatus == AttendanceStatus.Present || att.AttendanceStatus == AttendanceStatus.PresentDuringLeave)
                        ? "Present"
                        : (att.AttendanceStatus == AttendanceStatus.Late || att.AttendanceStatus == AttendanceStatus.LateDuringLeave)
                            ? "Late"
                            : att.AttendanceStatus.ToString();
                }
                return new MyAttendanceDayDto
                {
                    Date = date,
                    Status = status,
                    CheckIn = att?.CheckInTime,
                    CheckOut = att?.CheckOutTime
                };
            })
            .ToList();

        // ── Upcoming leaves ──
        var upcomingLeavesEntities = await _context.Leaves
            .AsNoTracking()
            .Where(l => l.EmployeeId == employee.Id
                && l.StartDate >= todayStart
                && !l.IsDeleted)
            .OrderBy(l => l.StartDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        var upcomingLeaves = upcomingLeavesEntities.Select(l => new UpcomingLeaveDto
        {
            Id = l.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            LeaveType = l.LeaveType.ToString(),
            From = DateOnly.FromDateTime(l.StartDate),
            To = DateOnly.FromDateTime(l.EndDate),
            Status = l.LeaveStatus.ToString()
        }).ToList();

        // ── Notifications ──
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == domainUserId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new DashboardNotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var dto = new MyDashboardDto
        {
            TodayStatus = todayStatus,
            CheckInTime = todayAttendance?.CheckInTime,
            CheckOutTime = todayAttendance?.CheckOutTime,
            TotalWorkingDaysThisMonth = workingDays,
            PresentDaysThisMonth = presentDays,
            LateDaysThisMonth = lateDays,
            AbsentDaysThisMonth = absentDays,
            AttendanceRateThisMonth = attendanceRate,
            TotalLeavesTaken = totalLeavesTaken,
            PendingLeaveRequests = pendingLeaveRequests,
            Last7Days = last7Days,
            UpcomingLeaves = upcomingLeaves,
            Notifications = notifications
        };

        return Result<MyDashboardDto>.Success(dto);
    }
}
