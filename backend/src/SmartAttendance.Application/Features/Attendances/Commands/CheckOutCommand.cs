using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class CheckOutResponse
{
    public Guid AttendanceId { get; set; }
    public DateTime CheckOutTime { get; set; }
}

public class CheckOutCommand : IRequest<Result<CheckOutResponse>>
{
    public Guid AttendanceId { get; set; }
    public int CheckOutMethod { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string DeviceInformation { get; set; } = string.Empty;
    public DateTime CheckOutTime { get; set; }
}

public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<CheckOutResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IGeoLocationService _geoLocationService;

    public CheckOutCommandHandler(
        IApplicationDbContext context,
        IGeoLocationService geoLocationService)
    {
        _context = context;
        _geoLocationService = geoLocationService;
    }

    public async Task<Result<CheckOutResponse>> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Shift)
            .Include(a => a.AttendancePolicy)
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);

        if (attendance == null)
            return Result<CheckOutResponse>.Failure(new[] { "Attendance record not found." });

        // ── Branch & Device GPS Geofencing Validation ─────────────────────────
        var employee = _context.Employees != null
            ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == attendance.EmployeeId, cancellationToken)
            : null;

        var branch = employee?.Branch ?? (employee != null && employee.BranchId != Guid.Empty && _context.Branches != null
            ? await _context.Branches.FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken)
            : null);

        var device = (attendance.AttendanceDeviceId != Guid.Empty && _context.AttendanceDevices != null)
            ? await _context.AttendanceDevices.FirstOrDefaultAsync(d => d.Id == attendance.AttendanceDeviceId, cancellationToken)
            : null;

        var validationResult = SmartAttendance.Application.Common.Helpers.LocationValidationHelper.ValidateLocation(
            request.Latitude,
            request.Longitude,
            branch,
            device,
            attendance.AttendancePolicy);

        if (!validationResult.Succeeded)
        {
            return Result<CheckOutResponse>.Failure(validationResult.Errors);
        }

        attendance.RecordCheckOut(
            request.CheckOutTime,
            (SmartAttendance.Domain.Enums.CheckInMethod)request.CheckOutMethod,
            request.Latitude,
            request.Longitude,
            request.IPAddress,
            request.DeviceInformation);

        // ⚠️ تحديد الحالة النهائية (Present/Late/EarlyLeave) لم يكن يحصل تلقائيًا
        // نحسب دقائق التأخير الفعلية بمقارنة وقت الدخول بوقت بداية الشفت + فترة السماح
        var shiftStart = attendance.CheckInTime.Date.Add(attendance.Shift.StartTime.ToTimeSpan());
        var shiftEnd = attendance.CheckInTime.Date.Add(attendance.Shift.EndTime.ToTimeSpan());

        var gracePeriod = attendance.AttendancePolicy.GracePeriodMinutes;
        var lateMinutes = Math.Max(0, (int)(attendance.CheckInTime - shiftStart).TotalMinutes - gracePeriod);

        var earlyLeaveTolerance = attendance.AttendancePolicy.EarlyLeaveToleranceMinutes;
        var earlyLeaveMinutes = Math.Max(0, (int)(shiftEnd - request.CheckOutTime).TotalMinutes - earlyLeaveTolerance);

        var overtimeMinutes = Math.Max(0, (int)(request.CheckOutTime - shiftEnd).TotalMinutes);

        var todayStartForLeave = request.CheckOutTime.Date;
        var todayEndForLeave = todayStartForLeave.AddDays(1).AddTicks(-1);
        var hasApprovedLeave = _context.Leaves != null && await _context.Leaves.AnyAsync(l => 
            l.EmployeeId == attendance.EmployeeId && 
            l.LeaveStatus == LeaveStatus.Approved && 
            l.StartDate <= todayEndForLeave && l.EndDate >= todayStartForLeave, 
            cancellationToken);

        var maxBreakDuration = attendance.Shift?.MaxBreakDurationMinutes ?? 60;
        attendance.CalculateWorkingHours(lateMinutes, earlyLeaveMinutes, breakMinutes: 0, overtimeMinutes: overtimeMinutes, isPresentDuringLeave: hasApprovedLeave, maxBreakDurationMinutes: maxBreakDuration);

        // ── Also persist a CheckOut AttendanceEvent for full audit trail ───────
        // This keeps event history consistent even when the legacy /check-out endpoint is used.
        var checkOutEvent = AttendanceEvent.Create(
            attendance.CompanyId,
            attendance.Id,
            attendance.EmployeeId,
            request.CheckOutTime,
            AttendanceEventType.CheckOut);
        await _context.AttendanceEvents.AddAsync(checkOutEvent, cancellationToken);

        var response = new CheckOutResponse
        {
            AttendanceId = attendance.Id,
            CheckOutTime = request.CheckOutTime
        };

        return Result<CheckOutResponse>.Success(response);
    }
}