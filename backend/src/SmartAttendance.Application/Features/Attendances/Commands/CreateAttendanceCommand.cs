using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class CreateAttendanceCommand : IRequest<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public Guid AttendanceDeviceId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime CheckInTime { get; set; }
    public CheckInMethod CheckInMethod { get; set; }
    public DateTime? CheckOutTime { get; set; }
}

public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateAttendanceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<Guid>.Failure(new[] { "Tenant ID is required." });
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.CompanyId == companyId, cancellationToken);

        if (employee == null)
        {
            return Result<Guid>.Failure(new[] { "Employee not found." });
        }

        var policyId = request.AttendancePolicyId != Guid.Empty
            ? request.AttendancePolicyId
            : employee.AttendancePolicyId;

        if (policyId == Guid.Empty)
        {
            var defaultPolicy = await _context.AttendancePolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CompanyId == companyId && !p.IsDeleted, cancellationToken);
            if (defaultPolicy != null) policyId = defaultPolicy.Id;
        }

        var shiftId = request.ShiftId;
        if (shiftId == Guid.Empty)
        {
            var defaultShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.IsActive && !s.IsDeleted, cancellationToken);
            if (defaultShift != null)
            {
                shiftId = defaultShift.Id;
            }
            else
            {
                var defaultSchedule = await _context.WorkSchedules
                    .FirstOrDefaultAsync(w => w.CompanyId == companyId && !w.IsDeleted, cancellationToken);

                var scheduleId = defaultSchedule?.Id ?? Guid.Empty;
                if (scheduleId == Guid.Empty)
                {
                    var newSchedule = SmartAttendance.Domain.Entities.WorkSchedule.Create(
                        companyId,
                        "Standard Schedule",
                        "Default company schedule",
                        false,
                        40,
                        15,
                        30,
                        30);
                    await _context.WorkSchedules.AddAsync(newSchedule, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    scheduleId = newSchedule.Id;
                }

                var newShift = SmartAttendance.Domain.Entities.Shift.Create(
                    companyId,
                    scheduleId,
                    "Morning Shift",
                    "SH-MORN",
                    "Standard morning shift",
                    new TimeOnly(8, 0),
                    new TimeOnly(17, 0),
                    Domain.Enums.ShiftType.Morning,
                    false,
                    8,
                    9,
                    15,
                    30,
                    30,
                    60);

                await _context.Shifts.AddAsync(newShift, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                shiftId = newShift.Id;
            }
        }

        var deviceId = request.AttendanceDeviceId;
        if (deviceId == Guid.Empty)
        {
            var defaultDevice = await _context.AttendanceDevices
                .FirstOrDefaultAsync(d => d.CompanyId == companyId && d.IsActive && !d.IsDeleted, cancellationToken);
            if (defaultDevice != null)
            {
                deviceId = defaultDevice.Id;
            }
            else
            {
                var webDevice = SmartAttendance.Domain.Entities.AttendanceDevice.Create(
                    companyId,
                    "Web Portal / System",
                    "DEV-WEB-01",
                    "SYSTEM-WEB-PORTAL",
                    Domain.Enums.DeviceType.Web,
                    "Web / Cloud",
                    "1.0.0",
                    employee.BranchId != Guid.Empty ? employee.BranchId : null,
                    0,
                    0,
                    10000,
                    null,
                    null,
                    requireDeviceApproval: false,
                    requireSecureConnection: false,
                    supportsFaceRecognition: false,
                    supportsFingerprint: false,
                    supportsGPS: false,
                    supportsOfflineAttendance: false,
                    supportsQRCode: false,
                    supportsNFC: false);

                webDevice.Activate();
                await _context.AttendanceDevices.AddAsync(webDevice, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                deviceId = webDevice.Id;
            }
        }

        if (policyId == Guid.Empty || shiftId == Guid.Empty || deviceId == Guid.Empty)
        {
            return Result<Guid>.Failure(new[] { "Attendance Policy, Shift, and Device must exist to record attendance." });
        }

        var todayStartForLeave = request.CheckInTime.Date;
        var todayEndForLeave = todayStartForLeave.AddDays(1).AddTicks(-1);
        var hasApprovedLeave = _context.Leaves != null && await _context.Leaves.AnyAsync(l => 
            l.EmployeeId == request.EmployeeId && 
            l.LeaveStatus == LeaveStatus.Approved && 
            l.StartDate <= todayEndForLeave && l.EndDate >= todayStartForLeave, 
            cancellationToken);

        var entity = SmartAttendance.Domain.Entities.Attendance.CreateCheckIn(
            companyId,
            request.EmployeeId,
            deviceId,
            policyId,
            shiftId,
            request.CheckInTime,
            request.CheckInMethod,
            isPresentDuringLeave: hasApprovedLeave
        );

        if (request.CheckOutTime.HasValue)
        {
            if (request.CheckOutTime.Value < request.CheckInTime)
            {
                return Result<Guid>.Failure(new[] { "Check-out time cannot be earlier than check-in time." });
            }
            entity.RecordCheckOut(request.CheckOutTime.Value, request.CheckInMethod);

            var policy = await _context.AttendancePolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);
            var shift = await _context.Shifts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == shiftId, cancellationToken);

            int lateMinutes = 0;
            int earlyLeaveMinutes = 0;
            int overtimeMinutes = 0;

            if (shift != null)
            {
                var shiftStart = entity.CheckInTime.Date.Add(shift.StartTime.ToTimeSpan());
                var shiftEnd = entity.CheckInTime.Date.Add(shift.EndTime.ToTimeSpan());

                var gracePeriod = policy?.GracePeriodMinutes ?? 0;
                lateMinutes = Math.Max(0, (int)(entity.CheckInTime - shiftStart).TotalMinutes - gracePeriod);

                var earlyLeaveTolerance = policy?.EarlyLeaveToleranceMinutes ?? 0;
                earlyLeaveMinutes = Math.Max(0, (int)(shiftEnd - request.CheckOutTime.Value).TotalMinutes - earlyLeaveTolerance);

                overtimeMinutes = Math.Max(0, (int)(request.CheckOutTime.Value - shiftEnd).TotalMinutes);
            }

            var maxBreakDuration = shift?.MaxBreakDurationMinutes ?? 60;
            entity.CalculateWorkingHours(lateMinutes, earlyLeaveMinutes, breakMinutes: 0, overtimeMinutes: overtimeMinutes, isPresentDuringLeave: hasApprovedLeave, maxBreakDurationMinutes: maxBreakDuration);
        }

        await _context.Attendances.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}
