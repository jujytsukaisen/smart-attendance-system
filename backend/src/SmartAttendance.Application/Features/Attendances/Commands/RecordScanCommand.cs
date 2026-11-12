using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Helpers;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.Commands;

/// <summary>
/// The response returned after a successful smart-scan operation.
/// </summary>
public class RecordScanResponse
{
    /// <summary>The daily attendance record identifier (created on CheckIn, reused on subsequent scans).</summary>
    public Guid AttendanceId { get; set; }

    /// <summary>The identifier of the newly created <see cref="AttendanceEvent"/>.</summary>
    public Guid EventId { get; set; }

    /// <summary>The state-machine action that was taken for this scan.</summary>
    public string ActionTaken { get; set; } = string.Empty;

    /// <summary>The resulting employee state after this scan.</summary>
    public string CurrentState { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the scan.</summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Smart-scan command — the single entry point for all employee scans throughout the workday.
/// Automatically resolves the correct state-machine action (CheckIn / BreakStart / BreakEnd / CheckOut)
/// based on the employee's current attendance state, without requiring the caller to specify the action.
/// </summary>
public class RecordScanCommand : IRequest<Result<RecordScanResponse>>
{
    /// <summary>The scanning employee.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Shift to assign when creating the attendance record (CheckIn only).</summary>
    public Guid ShiftId { get; set; }

    /// <summary>Attendance policy to enforce (CheckIn only).</summary>
    public Guid AttendancePolicyId { get; set; }

    /// <summary>Optional attendance device that captured the scan.</summary>
    public Guid? AttendanceDeviceId { get; set; }

    /// <summary>The check-in/verification method used for this scan.</summary>
    public int CheckInMethod { get; set; }

    /// <summary>Optional GPS latitude of the scan location.</summary>
    public double Latitude { get; set; }

    /// <summary>Optional GPS longitude of the scan location.</summary>
    public double Longitude { get; set; }

    /// <summary>Optional IP address from which the scan originated.</summary>
    public string IPAddress { get; set; } = string.Empty;

    /// <summary>Optional device information string.</summary>
    public string DeviceInformation { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the scan.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// When <c>true</c>, forces a <c>CheckOut</c> even if the employee is in <c>Working</c> state
    /// (i.e., they want to end the day without a preceding break).
    /// When <c>false</c> (default), a scan while Working triggers a <c>BreakStart</c>.
    /// </summary>
    public bool IsExplicitCheckOut { get; set; }

    /// <summary>Optional justification note (e.g. reason for early check-out).</summary>
    public string? Note { get; set; }
}

/// <summary>
/// Handles the <see cref="RecordScanCommand"/> by:
/// <list type="number">
///   <item>Loading today's attendance record (if any) with its events.</item>
///   <item>Computing the current <see cref="EmployeeAttendanceState"/> via <see cref="AttendanceStateHelper"/>.</item>
///   <item>Routing to the correct state-machine action.</item>
///   <item>Creating the new <see cref="AttendanceEvent"/> row.</item>
///   <item>Updating the parent <see cref="Attendance"/> record if needed (CheckIn creates it; CheckOut finalises it).</item>
/// </list>
/// </summary>
public class RecordScanCommandHandler : IRequestHandler<RecordScanCommand, Result<RecordScanResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeoLocationService _geoLocationService;

    public RecordScanCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGeoLocationService geoLocationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _geoLocationService = geoLocationService;
    }

    public async Task<Result<RecordScanResponse>> Handle(
        RecordScanCommand request,
        CancellationToken cancellationToken)
    {
        // ── Resolve Employee & Tenant ─────────────────────────────────────────
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Result<RecordScanResponse>.Failure(new[] { "Employee not found." });

        Guid companyId = employee.CompanyId;
        if (Guid.TryParse(_currentUserService.TenantId, out var parsedTenantId) && parsedTenantId != Guid.Empty)
        {
            companyId = parsedTenantId;
        }

        var checkInMethod = (CheckInMethod)request.CheckInMethod;

        // ── Load today's attendance + events ─────────────────────────────────
        var todayStart = request.Timestamp.Date;
        var todayEnd   = todayStart.AddDays(1).AddTicks(-1);

        var attendance = await _context.Attendances
            .Include(a => a.Events)
            .Include(a => a.Shift)
            .Include(a => a.AttendancePolicy)
            .FirstOrDefaultAsync(
                a => a.EmployeeId == request.EmployeeId
                  && a.CheckInTime >= todayStart
                  && a.CheckInTime <= todayEnd,
                cancellationToken);

        // ── Compute current state ────────────────────────────────────────────
        var currentState = attendance is null
            ? EmployeeAttendanceState.NotCheckedIn
            : (attendance.CheckOutTime.HasValue
                ? EmployeeAttendanceState.Finished
                : (!attendance.Events.Any()
                    ? EmployeeAttendanceState.Working
                    : AttendanceStateHelper.ComputeState(attendance.Events, attendance.CheckOutTime)));

        // ── Guard: no re-entry after CheckOut ────────────────────────────────
        if (currentState == EmployeeAttendanceState.Finished)
            return Result<RecordScanResponse>.Failure(new[]
            {
                "The attendance record for today is already closed. Check-out has been recorded."
            });

        // ── Branch & Device GPS Geofencing Validation ─────────────────────────
        var branch = employee.Branch ?? (employee.BranchId != Guid.Empty && _context.Branches != null
            ? await _context.Branches.FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken)
            : null);

        var policy = attendance?.AttendancePolicy
            ?? await _context.AttendancePolicies.FirstOrDefaultAsync(
                p => p.Id == (employee.AttendancePolicyId != Guid.Empty ? employee.AttendancePolicyId : request.AttendancePolicyId),
                cancellationToken);

        var device = (request.AttendanceDeviceId.HasValue && request.AttendanceDeviceId.Value != Guid.Empty)
            ? await _context.AttendanceDevices.FirstOrDefaultAsync(d => d.Id == request.AttendanceDeviceId.Value, cancellationToken)
            : null;

        var locationValidation = LocationValidationHelper.ValidateLocation(
            request.Latitude,
            request.Longitude,
            branch,
            device,
            policy,
            employee.Department?.RequiresLocationValidation ?? true);

        if (!locationValidation.Succeeded)
        {
            return Result<RecordScanResponse>.Failure(locationValidation.Errors);
        }

        // ── Route to correct action ──────────────────────────────────────────
        AttendanceEventType actionTaken;

        switch (currentState)
        {
            case EmployeeAttendanceState.NotCheckedIn:
            {
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

                    var detectedShift = ShiftMatchingHelper.DetectBestShift(availableShifts, request.Timestamp);
                    if (detectedShift != null)
                    {
                        resolvedShiftId = detectedShift.Id;
                    }
                }

                // ── Resolve valid AttendancePolicyId ───────────────────────
                Guid resolvedPolicyId = request.AttendancePolicyId;
                if (resolvedPolicyId == Guid.Empty || !await _context.AttendancePolicies.AnyAsync(p => p.Id == resolvedPolicyId, cancellationToken))
                {
                    if (employee.AttendancePolicyId != Guid.Empty && await _context.AttendancePolicies.AnyAsync(p => p.Id == employee.AttendancePolicyId, cancellationToken))
                    {
                        resolvedPolicyId = employee.AttendancePolicyId;
                    }
                    else
                    {
                        resolvedPolicyId = await _context.AttendancePolicies
                            .Where(p => p.CompanyId == companyId)
                            .Select(p => p.Id)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (resolvedPolicyId == Guid.Empty)
                        {
                            resolvedPolicyId = await _context.AttendancePolicies
                                .Select(p => p.Id)
                                .FirstOrDefaultAsync(cancellationToken);
                        }
                    }
                }

                // ── Resolve valid AttendanceDeviceId ───────────────────────
                Guid resolvedDeviceId = Guid.Empty;
                if (request.AttendanceDeviceId.HasValue && request.AttendanceDeviceId.Value != Guid.Empty &&
                    await _context.AttendanceDevices.AnyAsync(d => d.Id == request.AttendanceDeviceId.Value, cancellationToken))
                {
                    resolvedDeviceId = request.AttendanceDeviceId.Value;
                }
                else
                {
                    resolvedDeviceId = await _context.AttendanceDevices
                        .Where(d => d.CompanyId == companyId)
                        .Select(d => d.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (resolvedDeviceId == Guid.Empty)
                    {
                        resolvedDeviceId = await _context.AttendanceDevices
                            .Select(d => d.Id)
                            .FirstOrDefaultAsync(cancellationToken);
                    }
                }

                if (resolvedShiftId == Guid.Empty || resolvedPolicyId == Guid.Empty || resolvedDeviceId == Guid.Empty)
                {
                    return Result<RecordScanResponse>.Failure(new[]
                    {
                        "Cannot record attendance: required shift, policy, or device configuration is missing in the system."
                    });
                }

                // ── First scan of the day: CheckIn ──────────────────────────
                actionTaken = AttendanceEventType.CheckIn;

                var todayStartForLeave = request.Timestamp.Date;
                var todayEndForLeave = todayStartForLeave.AddDays(1).AddTicks(-1);
                var hasApprovedLeave = _context.Leaves != null && await _context.Leaves.AnyAsync(l => 
                    l.EmployeeId == request.EmployeeId && 
                    l.LeaveStatus == LeaveStatus.Approved && 
                    l.StartDate <= todayEndForLeave && l.EndDate >= todayStartForLeave, 
                    cancellationToken);

                // ✅ قراءة فترة السماح من إعدادات قاعدة البيانات (Attendance Settings → GracePeriodMinutes)
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

                var shift = await _context.Shifts.FindAsync(new object[] { resolvedShiftId }, cancellationToken);
                bool isLate = shift != null && TimeOnly.FromDateTime(request.Timestamp) > shift.StartTime.AddMinutes(companyGrace);

                attendance = Attendance.CreateCheckIn(
                    companyId,
                    request.EmployeeId,
                    resolvedDeviceId,
                    resolvedPolicyId,
                    resolvedShiftId,
                    request.Timestamp,
                    checkInMethod,
                    request.Latitude,
                    request.Longitude,
                    request.IPAddress,
                    request.DeviceInformation,
                    isPresentDuringLeave: hasApprovedLeave,
                    isLate: isLate);

                await _context.Attendances.AddAsync(attendance, cancellationToken);
                break;
            }

            case EmployeeAttendanceState.Working when request.IsExplicitCheckOut:
            {
                // ── Explicit end-of-day checkout while Working ──────────────
                actionTaken = AttendanceEventType.CheckOut;
                await FinaliseCheckOut(attendance!, request, cancellationToken);
                break;
            }

            case EmployeeAttendanceState.Working:
            {
                // ── Second scan while Working → BreakStart ──────────────────
                actionTaken = AttendanceEventType.BreakStart;
                break;
            }

            case EmployeeAttendanceState.OnBreak:
            {
                // ── Scan while OnBreak → BreakEnd (return from break) ───────
                actionTaken = AttendanceEventType.BreakEnd;
                break;
            }

            default:
                return Result<RecordScanResponse>.Failure(new[] { "Unexpected attendance state." });
        }

        // ── Create the event row ─────────────────────────────────────────────
        var evt = AttendanceEvent.Create(
            companyId,
            attendance!.Id,
            request.EmployeeId,
            request.Timestamp,
            actionTaken,
            request.Note);

        await _context.AttendanceEvents.AddAsync(evt, cancellationToken);

        // ── Determine result state ───────────────────────────────────────────
        var resultState = actionTaken switch
        {
            AttendanceEventType.CheckIn    => EmployeeAttendanceState.Working,
            AttendanceEventType.BreakStart => EmployeeAttendanceState.OnBreak,
            AttendanceEventType.BreakEnd   => EmployeeAttendanceState.Working,
            AttendanceEventType.CheckOut   => EmployeeAttendanceState.Finished,
            _                              => EmployeeAttendanceState.NotCheckedIn
        };

        return Result<RecordScanResponse>.Success(new RecordScanResponse
        {
            AttendanceId = attendance.Id,
            EventId      = evt.Id,
            ActionTaken  = actionTaken.ToString(),
            CurrentState = resultState.ToString(),
            Timestamp    = request.Timestamp
        });
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Finalises the attendance record on a CheckOut scan.
    /// Computes late/early-leave/overtime minutes from the loaded Shift and Policy,
    /// then calls UpdateSummaryFromEvents after adding the CheckOut event.
    /// </summary>
    private async Task FinaliseCheckOut(
        Attendance attendance,
        RecordScanCommand request,
        CancellationToken cancellationToken)
    {
        var checkInMethod = (CheckInMethod)request.CheckInMethod;

        attendance.RecordCheckOutFromEvent(
            request.Timestamp,
            checkInMethod,
            request.Latitude,
            request.Longitude,
            request.IPAddress,
            request.DeviceInformation);

        // ── Policy-based calculations ────────────────────────────────────────
        int lateMinutes = 0, earlyLeaveMinutes = 0, overtimeMinutes = 0;

        if (attendance.Shift != null && attendance.AttendancePolicy != null)
        {
            var shiftStart  = attendance.CheckInTime.Date.Add(attendance.Shift.StartTime.ToTimeSpan());
            var shiftEnd    = attendance.CheckInTime.Date.Add(attendance.Shift.EndTime.ToTimeSpan());

            // Handle shifts crossing midnight
            if (shiftEnd <= shiftStart)
            {
                shiftEnd = shiftEnd.AddDays(1);
            }

            var gracePeriod = attendance.AttendancePolicy.GracePeriodMinutes;
            var earlyLeaveTolerance = attendance.AttendancePolicy.EarlyLeaveToleranceMinutes;

            lateMinutes       = Math.Max(0, (int)(attendance.CheckInTime - shiftStart).TotalMinutes - gracePeriod);
            earlyLeaveMinutes = Math.Max(0, (int)(shiftEnd - request.Timestamp).TotalMinutes - earlyLeaveTolerance);
            overtimeMinutes   = Math.Max(0, (int)(request.Timestamp - shiftEnd).TotalMinutes);
        }

        // ── Build a synthetic events list including the about-to-be-saved CheckOut ──
        // We add the virtual CheckOut event so UpdateSummaryFromEvents can pair it with breaks.
        // The actual AttendanceEvent row is inserted by the caller after this method.
        var virtualEvents = attendance.Events.ToList();
        virtualEvents.Add(AttendanceEvent.Create(
            attendance.CompanyId,
            attendance.Id,
            attendance.EmployeeId,
            request.Timestamp,
            AttendanceEventType.CheckOut));

        // Temporarily expose events via reflection to call UpdateSummaryFromEvents
        // without breaking encapsulation — we reload events from DB in subsequent queries.
        // Instead, calculate directly here without the domain method to avoid reflection.
        int totalBreakMinutes = 0;
        DateTime? breakStart = null;
        foreach (var evt in virtualEvents.OrderBy(e => e.Timestamp))
        {
            if (evt.EventType == AttendanceEventType.BreakStart)
                breakStart = evt.Timestamp;
            else if (evt.EventType == AttendanceEventType.BreakEnd && breakStart.HasValue)
            {
                totalBreakMinutes += (int)(evt.Timestamp - breakStart.Value).TotalMinutes;
                breakStart = null;
            }
        }

        var todayStartForLeave = request.Timestamp.Date;
        var todayEndForLeave = todayStartForLeave.AddDays(1).AddTicks(-1);
        var hasApprovedLeave = _context.Leaves != null && await _context.Leaves.AnyAsync(l => 
            l.EmployeeId == request.EmployeeId && 
            l.LeaveStatus == LeaveStatus.Approved && 
            l.StartDate <= todayEndForLeave && l.EndDate >= todayStartForLeave, 
            cancellationToken);

        var maxBreakDuration = attendance.Shift?.MaxBreakDurationMinutes ?? 60;
        attendance.CalculateWorkingHours(lateMinutes, earlyLeaveMinutes, totalBreakMinutes, overtimeMinutes, hasApprovedLeave, maxBreakDuration);
    }
}
