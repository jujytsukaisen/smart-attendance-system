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

public class CheckInResponse
{
    public Guid AttendanceId { get; set; }
    public DateTime CheckInTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CheckInCommand : IRequest<Result<CheckInResponse>>
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    public Guid? AttendanceDeviceId { get; set; }
    public int CheckInMethod { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string DeviceInformation { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
}

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<CheckInResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFaceRecognitionService _faceRecognitionService;
    private readonly IGeoLocationService _geoLocationService;
    private readonly ICurrentUserService _currentUserService;

    public CheckInCommandHandler(
        IApplicationDbContext context, 
        IFaceRecognitionService faceRecognitionService,
        IGeoLocationService geoLocationService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _faceRecognitionService = faceRecognitionService;
        _geoLocationService = geoLocationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CheckInResponse>> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<CheckInResponse>.Failure(new[] { "Tenant ID is missing or invalid." });
        }

        // ── Branch & Device GPS Geofencing Validation ─────────────────────────
        var employee = _context.Employees != null
            ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken)
            : null;

        var branch = employee?.Branch ?? (employee != null && employee.BranchId != Guid.Empty && _context.Branches != null
            ? await _context.Branches.FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken)
            : null);

        var attendancePolicy = _context.AttendancePolicies != null
            ? await _context.AttendancePolicies
                .FirstOrDefaultAsync(p => p.Id == (employee != null && employee.AttendancePolicyId != Guid.Empty ? employee.AttendancePolicyId : request.AttendancePolicyId), cancellationToken)
            : null;

        var device = (request.AttendanceDeviceId.HasValue && request.AttendanceDeviceId.Value != Guid.Empty && _context.AttendanceDevices != null)
            ? await _context.AttendanceDevices.FirstOrDefaultAsync(d => d.Id == request.AttendanceDeviceId.Value, cancellationToken)
            : null;

        var validationResult = SmartAttendance.Application.Common.Helpers.LocationValidationHelper.ValidateLocation(
            request.Latitude,
            request.Longitude,
            branch,
            device,
            attendancePolicy);

        if (!validationResult.Succeeded)
        {
            return Result<CheckInResponse>.Failure(validationResult.Errors);
        }

        // ── Resolve valid ShiftId with Smart Auto-Detection ───────
        Guid resolvedShiftId = request.ShiftId;
        if (resolvedShiftId == Guid.Empty || !await _context.Shifts.AnyAsync(s => s.Id == resolvedShiftId, cancellationToken))
        {
            var availableShifts = await _context.Shifts
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .ToListAsync(cancellationToken);

            if (availableShifts.Count == 0)
            {
                availableShifts = await _context.Shifts
                    .Where(s => s.IsActive)
                    .ToListAsync(cancellationToken);
            }

            var detectedShift = SmartAttendance.Application.Common.Helpers.ShiftMatchingHelper.DetectBestShift(availableShifts, request.CheckInTime);
            if (detectedShift != null)
            {
                resolvedShiftId = detectedShift.Id;
            }
        }

        var todayStart = request.CheckInTime.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);
        var hasApprovedLeave = _context.Leaves != null && await _context.Leaves.AnyAsync(l => 
            l.EmployeeId == request.EmployeeId && 
            l.LeaveStatus == LeaveStatus.Approved && 
            l.StartDate <= todayEnd && l.EndDate >= todayStart, 
            cancellationToken);

        // ✅ قراءة فترة السماح من إعدادات قاعدة البيانات (Attendance Settings → GracePeriodMinutes)
        // هذا يجعل إعداد "Grace Period" في صفحة الإعدادات يؤثر فعلاً على تحديد التأخر
        var companyGrace = 0;
        try
        {
            var graceStr = await _context.AppSettings
                .AsNoTracking()
                .Where(s => s.CompanyId == companyId && s.Category == "Attendance" && s.Key == "GracePeriodMinutes")
                .Select(s => s.Value)
                .FirstOrDefaultAsync(cancellationToken);
            if (graceStr is not null && int.TryParse(graceStr, out var parsed))
                companyGrace = parsed;
        }
        catch { /* استخدم 0 كقيمة افتراضية إذا فشلت قراءة الإعداد */ }

        var shift = await _context.Shifts.FindAsync(resolvedShiftId);
        bool isLate = shift != null &&
                      TimeOnly.FromDateTime(request.CheckInTime) > shift.StartTime.AddMinutes(companyGrace);


        var attendance = SmartAttendance.Domain.Entities.Attendance.CreateCheckIn(
            companyId,
            request.EmployeeId,
            request.AttendanceDeviceId ?? Guid.NewGuid(),
            request.AttendancePolicyId,
            resolvedShiftId,
            request.CheckInTime,
            (SmartAttendance.Domain.Enums.CheckInMethod)request.CheckInMethod,
            request.Latitude,
            request.Longitude,
            request.IPAddress,
            request.DeviceInformation,
            isPresentDuringLeave: hasApprovedLeave,
            isLate: isLate);

        await _context.Attendances.AddAsync(attendance, cancellationToken);

        // ── Also persist a CheckIn AttendanceEvent for full audit trail ──────
        // This keeps event history consistent even when the legacy /check-in endpoint is used.
        var checkInEvent = AttendanceEvent.Create(
            companyId,
            attendance.Id,
            request.EmployeeId,
            request.CheckInTime,
            AttendanceEventType.CheckIn);
        await _context.AttendanceEvents.AddAsync(checkInEvent, cancellationToken);
        // SaveChangesAsync is handled by TransactionBehavior

        var response = new CheckInResponse
        {
            AttendanceId = attendance.Id,
            CheckInTime = request.CheckInTime,
            Status = "CheckedIn"
        };
        return Result<CheckInResponse>.Success(response);
    }
}
