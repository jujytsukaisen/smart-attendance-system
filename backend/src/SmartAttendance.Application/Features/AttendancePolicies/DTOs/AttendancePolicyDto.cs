using System;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.AttendancePolicies.DTOs;

public class AttendancePolicyDto : IMapFrom<AttendancePolicy>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public SmartAttendance.Domain.Enums.AttendanceMode AttendanceMode { get; set; }
    
    public bool RequireFaceRecognition { get; set; }
    public bool RequireGPS { get; set; }
    public bool RequireApprovedDevice { get; set; }
    public bool RequireCompanyNetwork { get; set; }
    public bool AllowMobileCheckIn { get; set; }
    public bool AllowDesktopCheckIn { get; set; }
    public bool AllowWebCheckIn { get; set; }
    public bool AllowOfflineAttendance { get; set; }
    public double AllowedRadiusMeters { get; set; }
    public bool AllowMultipleCheckIn { get; set; }
    public bool AllowManualAttendance { get; set; }
    public bool RequireManagerApproval { get; set; }
    public double MaximumDistanceMeters { get; set; }
    
    public int LateToleranceMinutes { get; set; }
    public int EarlyLeaveToleranceMinutes { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int OvertimeThresholdMinutes { get; set; }
    public int MinimumOvertimeMinutes { get; set; }
    public int MaximumOvertimeMinutes { get; set; }
    
    public bool AutoCheckoutEnabled { get; set; }
    public TimeOnly? AutoCheckoutTime { get; set; }
    
    public bool MultipleShiftsSupport { get; set; }
    
    public SmartAttendance.Domain.Enums.RoundTimeRule RoundTimeRule { get; set; }
    public SmartAttendance.Domain.Enums.MissingPunchPolicy MissingPunchPolicy { get; set; }
    public SmartAttendance.Domain.Enums.WeekendPolicy WeekendPolicy { get; set; }
    public SmartAttendance.Domain.Enums.HolidayPolicy HolidayPolicy { get; set; }
    
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
