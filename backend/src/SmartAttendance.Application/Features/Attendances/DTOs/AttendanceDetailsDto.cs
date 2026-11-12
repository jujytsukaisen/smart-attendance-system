using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Helpers;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.DTOs;

public class AttendanceDetailsDto : IMapFrom<Attendance>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public Guid AttendanceDeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public Guid AttendancePolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    
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
    public int ExcessBreakMinutes { get; set; }

    // Validations
    public FaceVerificationStatus FaceVerificationStatus { get; set; }
    public GPSVerificationStatus GPSVerificationStatus { get; set; }
    public DeviceVerificationStatus DeviceVerificationStatus { get; set; }
    public ManagerApprovalStatus ManagerApprovalStatus { get; set; }

    // ── Multi-Scan State Machine ──────────────────────────────────────────────

    /// <summary>
    /// Ordered list of all scan events for this attendance record.
    /// Provides the full daily timeline: CheckIn → BreakStart → BreakEnd → CheckOut.
    /// </summary>
    public List<AttendanceEventDto> Timeline { get; set; } = new();

    /// <summary>
    /// Derived current state of the employee for this attendance day.
    /// Values: "NotCheckedIn" | "Working" | "OnBreak" | "Finished".
    /// </summary>
    public string CurrentState { get; set; } = "NotCheckedIn";

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Attendance, AttendanceDetailsDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? (s.Employee.FirstName + " " + s.Employee.LastName).Trim() : string.Empty))
            .ForMember(d => d.EmployeeNumber, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.EmployeeNumber : string.Empty))
            .ForMember(d => d.ShiftName, opt => opt.MapFrom(s => s.Shift != null ? s.Shift.Name : string.Empty))
            .ForMember(d => d.DeviceName, opt => opt.MapFrom(s => s.AttendanceDevice != null ? s.AttendanceDevice.Name : string.Empty))
            .ForMember(d => d.PolicyName, opt => opt.MapFrom(s => s.AttendancePolicy != null ? s.AttendancePolicy.Name : string.Empty))
            .ForMember(d => d.Timeline, opt => opt.MapFrom(s =>
                s.Events.OrderBy(e => e.Timestamp).Select(e => new AttendanceEventDto
                {
                    Id           = e.Id,
                    AttendanceId = e.AttendanceId,
                    Timestamp    = e.Timestamp,
                    EventType    = e.EventType,
                    Note         = e.Note
                }).ToList()))
            .ForMember(d => d.CurrentState, opt => opt.MapFrom(s =>
                AttendanceStateHelper.ComputeState(s.Events, s.CheckOutTime).ToString()));
    }
}
