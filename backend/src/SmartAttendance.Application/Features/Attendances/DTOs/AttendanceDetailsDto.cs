using System;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.DTOs;

public class AttendanceDetailsDto : IMapFrom<Attendance>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid AttendanceDeviceId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    
    // Check In
    public DateTime CheckInTime { get; set; }
    public CheckInMethod CheckInMethod { get; set; }
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public string? CheckInIPAddress { get; set; }
    public string? CheckInDeviceInfo { get; set; }
    public string? CheckInPhotoReference { get; set; }

    // Check Out
    public DateTime? CheckOutTime { get; set; }
    public CheckInMethod? CheckOutMethod { get; set; }
    public double? CheckOutLatitude { get; set; }
    public double? CheckOutLongitude { get; set; }
    public string? CheckOutIPAddress { get; set; }
    public string? CheckOutDeviceInfo { get; set; }
    public string? CheckOutPhotoReference { get; set; }

    // Calculations
    public AttendanceStatus AttendanceStatus { get; set; }
    public int WorkingMinutes { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public int BreakMinutes { get; set; }

    // Validations
    public FaceVerificationStatus FaceVerificationStatus { get; set; }
    public GPSVerificationStatus GPSVerificationStatus { get; set; }
    public DeviceVerificationStatus DeviceVerificationStatus { get; set; }
    public ManagerApprovalStatus ManagerApprovalStatus { get; set; }
}
