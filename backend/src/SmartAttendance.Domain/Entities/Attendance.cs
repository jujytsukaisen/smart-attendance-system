using System;
using System.Collections.Generic;
using System.Linq;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Attendance : BaseTenantEntity
{
    // Identity
    public Guid EmployeeId { get; private set; }
    public Guid AttendanceDeviceId { get; private set; }
    public Guid AttendancePolicyId { get; private set; }
    public Guid ShiftId { get; private set; }

    // Check In
    public DateTime CheckInTime { get; private set; }
    public CheckInMethod CheckInMethod { get; private set; }
    public double? CheckInLatitude { get; private set; }
    public double? CheckInLongitude { get; private set; }
    public string? CheckInIPAddress { get; private set; }
    public string? CheckInDeviceInfo { get; private set; }
    public string? CheckInPhotoReference { get; private set; }

    // Check Out
    public DateTime? CheckOutTime { get; private set; }
    public CheckInMethod? CheckOutMethod { get; private set; }
    public double? CheckOutLatitude { get; private set; }
    public double? CheckOutLongitude { get; private set; }
    public string? CheckOutIPAddress { get; private set; }
    public string? CheckOutDeviceInfo { get; private set; }
    public string? CheckOutPhotoReference { get; private set; }

    // Calculated Information
    public AttendanceStatus AttendanceStatus { get; private set; }
    public int WorkingMinutes { get; private set; }
    public int LateMinutes { get; private set; }
    public int EarlyLeaveMinutes { get; private set; }
    public int OvertimeMinutes { get; private set; }
    public int BreakMinutes { get; private set; }
    public int ExcessBreakMinutes { get; private set; }

    // Verification
    public FaceVerificationStatus FaceVerificationStatus { get; private set; }
    public GPSVerificationStatus GPSVerificationStatus { get; private set; }
    public DeviceVerificationStatus DeviceVerificationStatus { get; private set; }
    public ManagerApprovalStatus ManagerApprovalStatus { get; private set; }

    // Relationships
    public Employee Employee { get; private set; } = null!;
    public AttendanceDevice AttendanceDevice { get; private set; } = null!;
    public AttendancePolicy AttendancePolicy { get; private set; } = null!;
    public Shift Shift { get; private set; } = null!;

    /// <summary>
    /// Ordered collection of every scan event for this daily attendance record.
    /// The sequence drives the state machine: CheckIn → BreakStart ⇄ BreakEnd → CheckOut.
    /// </summary>
    public IReadOnlyCollection<AttendanceEvent> Events { get; private set; } = new List<AttendanceEvent>();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Attendance() { }
#pragma warning restore CS8618

    private Attendance(
        Guid companyId,
        Guid employeeId,
        Guid attendanceDeviceId,
        Guid attendancePolicyId,
        Guid shiftId,
        DateTime checkInTime,
        CheckInMethod checkInMethod,
        double? checkInLatitude,
        double? checkInLongitude,
        string? checkInIPAddress,
        string? checkInDeviceInfo,
        string? checkInPhotoReference)
    {
        CompanyId = companyId;
        EmployeeId = employeeId;
        AttendanceDeviceId = attendanceDeviceId;
        AttendancePolicyId = attendancePolicyId;
        ShiftId = shiftId;
        CheckInTime = checkInTime;
        CheckInMethod = checkInMethod;
        CheckInLatitude = checkInLatitude;
        CheckInLongitude = checkInLongitude;
        CheckInIPAddress = checkInIPAddress;
        CheckInDeviceInfo = checkInDeviceInfo;
        CheckInPhotoReference = checkInPhotoReference;

        AttendanceStatus = AttendanceStatus.Pending;
        FaceVerificationStatus = FaceVerificationStatus.Pending;
        GPSVerificationStatus = GPSVerificationStatus.Pending;
        DeviceVerificationStatus = DeviceVerificationStatus.Pending;
        ManagerApprovalStatus = ManagerApprovalStatus.Pending;
    }

    public static Attendance CreateCheckIn(
        Guid companyId,
        Guid employeeId,
        Guid attendanceDeviceId,
        Guid attendancePolicyId,
        Guid shiftId,
        DateTime checkInTime,
        CheckInMethod checkInMethod,
        double? checkInLatitude = null,
        double? checkInLongitude = null,
        string? checkInIPAddress = null,
        string? checkInDeviceInfo = null,
        string? checkInPhotoReference = null,
        bool isPresentDuringLeave = false,
        bool isLate = false)
    {
        if (companyId == Guid.Empty)
            throw new InvalidAttendanceException("Company ID is required.");
        if (employeeId == Guid.Empty)
            throw new InvalidAttendanceException("Employee ID is required.");
        if (attendanceDeviceId == Guid.Empty)
            throw new InvalidAttendanceException("Attendance Device ID is required.");
        if (attendancePolicyId == Guid.Empty)
            throw new InvalidAttendanceException("Attendance Policy ID is required.");
        if (shiftId == Guid.Empty)
            throw new InvalidAttendanceException("Shift ID is required.");

        return new Attendance(
            companyId,
            employeeId,
            attendanceDeviceId,
            attendancePolicyId,
            shiftId,
            checkInTime,
            checkInMethod,
            checkInLatitude,
            checkInLongitude,
            checkInIPAddress,
            checkInDeviceInfo,
            checkInPhotoReference)
        {
            AttendanceStatus = isPresentDuringLeave 
                ? (isLate ? AttendanceStatus.LateDuringLeave : AttendanceStatus.PresentDuringLeave) 
                : (isLate ? AttendanceStatus.Late : AttendanceStatus.Pending)
        };
    }

    public void RecordCheckOut(
        DateTime checkOutTime,
        CheckInMethod checkOutMethod,
        double? checkOutLatitude = null,
        double? checkOutLongitude = null,
        string? checkOutIPAddress = null,
        string? checkOutDeviceInfo = null,
        string? checkOutPhotoReference = null)
    {
        if (CheckOutTime.HasValue)
            throw new InvalidAttendanceException("Cannot check out twice.");

        if (checkOutTime < CheckInTime)
            throw new InvalidAttendanceException("Check-out cannot occur before check-in.");

        CheckOutTime = checkOutTime;
        CheckOutMethod = checkOutMethod;
        CheckOutLatitude = checkOutLatitude;
        CheckOutLongitude = checkOutLongitude;
        CheckOutIPAddress = checkOutIPAddress;
        CheckOutDeviceInfo = checkOutDeviceInfo;
        CheckOutPhotoReference = checkOutPhotoReference;
    }

    public void Approve()
    {
        if (ManagerApprovalStatus == ManagerApprovalStatus.Rejected)
            throw new InvalidAttendanceException("Cannot approve rejected attendance.");

        ManagerApprovalStatus = ManagerApprovalStatus.Approved;
    }

    public void Reject()
    {
        if (ManagerApprovalStatus == ManagerApprovalStatus.Approved)
            throw new InvalidAttendanceException("Cannot reject approved attendance.");

        ManagerApprovalStatus = ManagerApprovalStatus.Rejected;
    }

    public void MarkFaceVerified(bool isVerified)
    {
        FaceVerificationStatus = isVerified ? FaceVerificationStatus.Verified : FaceVerificationStatus.Failed;
    }

    public void MarkGPSVerified(bool isVerified)
    {
        GPSVerificationStatus = isVerified ? GPSVerificationStatus.Verified : GPSVerificationStatus.Failed;
    }

    public void MarkDeviceVerified(bool isVerified)
    {
        DeviceVerificationStatus = isVerified ? DeviceVerificationStatus.Verified : DeviceVerificationStatus.Failed;
    }

    public void CalculateWorkingHours(int lateMinutes, int earlyLeaveMinutes, int breakMinutes, int overtimeMinutes, bool isPresentDuringLeave = false, int maxBreakDurationMinutes = 60)
    {
        if (!CheckOutTime.HasValue)
            throw new InvalidAttendanceException("Cannot calculate working hours without a check-out time.");

        var totalMinutes = (int)(CheckOutTime.Value - CheckInTime).TotalMinutes;
        var calculatedWorkingMinutes = totalMinutes - breakMinutes;

        if (calculatedWorkingMinutes < 0)
            throw new InvalidAttendanceException("Working minutes cannot be negative.");

        if (overtimeMinutes < 0)
            throw new InvalidAttendanceException("Overtime cannot be negative.");

        WorkingMinutes = calculatedWorkingMinutes;
        LateMinutes = lateMinutes;
        EarlyLeaveMinutes = earlyLeaveMinutes;
        BreakMinutes = breakMinutes;
        OvertimeMinutes = overtimeMinutes;
        ExcessBreakMinutes = Math.Max(0, breakMinutes - maxBreakDurationMinutes);

        if (isPresentDuringLeave)
            AttendanceStatus = lateMinutes > 0 ? AttendanceStatus.LateDuringLeave : AttendanceStatus.PresentDuringLeave;
        else if (lateMinutes > 0)
            AttendanceStatus = AttendanceStatus.Late;
        else if (earlyLeaveMinutes > 0)
            AttendanceStatus = AttendanceStatus.EarlyLeave;
        else
            AttendanceStatus = AttendanceStatus.Present;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    /// <summary>
    /// Records the final check-out as part of the multi-scan workflow.
    /// Unlike <see cref="RecordCheckOut"/>, this variant is called by the state-machine handler
    /// and does NOT recalculate working hours (that is done separately via <see cref="UpdateSummaryFromEvents"/>).
    /// </summary>
    /// <param name="checkOutTime">UTC time of the check-out scan.</param>
    /// <param name="checkOutMethod">The method used for the check-out scan.</param>
    /// <param name="checkOutLatitude">Optional GPS latitude.</param>
    /// <param name="checkOutLongitude">Optional GPS longitude.</param>
    /// <param name="checkOutIPAddress">Optional IP address.</param>
    /// <param name="checkOutDeviceInfo">Optional device information string.</param>
    /// <param name="checkOutPhotoReference">Optional photo reference key.</param>
    /// <exception cref="InvalidAttendanceException">Thrown if already checked out.</exception>
    public void RecordCheckOutFromEvent(
        DateTime checkOutTime,
        CheckInMethod checkOutMethod,
        double? checkOutLatitude = null,
        double? checkOutLongitude = null,
        string? checkOutIPAddress = null,
        string? checkOutDeviceInfo = null,
        string? checkOutPhotoReference = null)
    {
        if (CheckOutTime.HasValue)
            throw new InvalidAttendanceException("Cannot check out twice on the same attendance record.");

        if (checkOutTime < CheckInTime)
            throw new InvalidAttendanceException("Check-out cannot occur before check-in.");

        CheckOutTime         = checkOutTime;
        CheckOutMethod       = checkOutMethod;
        CheckOutLatitude     = checkOutLatitude;
        CheckOutLongitude    = checkOutLongitude;
        CheckOutIPAddress    = checkOutIPAddress;
        CheckOutDeviceInfo   = checkOutDeviceInfo;
        CheckOutPhotoReference = checkOutPhotoReference;
    }

    /// <summary>
    /// Recalculates <see cref="BreakMinutes"/> and <see cref="WorkingMinutes"/> from the
    /// ordered events collection. Call this after a <c>CheckOut</c> event.
    /// </summary>
    /// <param name="events">Optional list of events. If null, the <see cref="Events"/> navigation property is used.</param>
    /// <param name="lateMinutes">Minutes the employee arrived late (policy-based, caller-supplied).</param>
    /// <param name="earlyLeaveMinutes">Minutes the employee left early (policy-based, caller-supplied).</param>
    /// <param name="overtimeMinutes">Overtime minutes (policy-based, caller-supplied).</param>
    /// <exception cref="InvalidAttendanceException">Thrown when check-out time is not set.</exception>
    public void UpdateSummaryFromEvents(
        IReadOnlyList<AttendanceEvent>? events = null,
        int lateMinutes = 0,
        int earlyLeaveMinutes = 0,
        int overtimeMinutes = 0,
        bool isPresentDuringLeave = false,
        int maxBreakDurationMinutes = 60)
    {
        if (!CheckOutTime.HasValue)
            throw new InvalidAttendanceException("Cannot calculate working hours without a check-out time.");

        // ── Calculate TotalBreakMinutes from paired BreakStart / BreakEnd events ──
        var sourceEvents = events ?? Events;
        var orderedEvents = sourceEvents.OrderBy(e => e.Timestamp).ToList();

        int totalBreakMinutes = 0;
        DateTime? breakStart = null;

        foreach (var evt in orderedEvents)
        {
            if (evt.EventType == AttendanceEventType.BreakStart)
            {
                breakStart = evt.Timestamp;
            }
            else if (evt.EventType == AttendanceEventType.BreakEnd && breakStart.HasValue)
            {
                totalBreakMinutes += (int)(evt.Timestamp - breakStart.Value).TotalMinutes;
                breakStart = null;
            }
        }

        // ── Calculate WorkingMinutes = (CheckOut - CheckIn) - TotalBreakMinutes ──
        var grossMinutes = (int)(CheckOutTime.Value - CheckInTime).TotalMinutes;
        var workedMinutes = Math.Max(0, grossMinutes - totalBreakMinutes);

        WorkingMinutes   = workedMinutes;
        BreakMinutes     = totalBreakMinutes;
        LateMinutes      = lateMinutes;
        EarlyLeaveMinutes = earlyLeaveMinutes;
        OvertimeMinutes  = overtimeMinutes;
        ExcessBreakMinutes = Math.Max(0, totalBreakMinutes - maxBreakDurationMinutes);

        if (isPresentDuringLeave)
            AttendanceStatus = lateMinutes > 0 ? AttendanceStatus.LateDuringLeave : AttendanceStatus.PresentDuringLeave;
        else if (lateMinutes > 0)
            AttendanceStatus = AttendanceStatus.Late;
        else if (earlyLeaveMinutes > 0)
            AttendanceStatus = AttendanceStatus.EarlyLeave;
        else
            AttendanceStatus = AttendanceStatus.Present;
    }
}
