using System;
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
        string? checkInPhotoReference = null)
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
            checkInPhotoReference);
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

    public void CalculateWorkingHours(int lateMinutes, int earlyLeaveMinutes, int breakMinutes, int overtimeMinutes)
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

        if (lateMinutes > 0)
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
}
