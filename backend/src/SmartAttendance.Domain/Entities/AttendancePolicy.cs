using System;
using System.Collections.Generic;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class AttendancePolicy : BaseTenantEntity
{
    private readonly List<Employee> _employees = new();

    // Identity
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }

    // Attendance Rules
    public AttendanceMode AttendanceMode { get; private set; }
    public bool RequireFaceRecognition { get; private set; }
    public bool RequireGPS { get; private set; }
    public bool RequireApprovedDevice { get; private set; }
    public bool RequireCompanyNetwork { get; private set; }
    public bool AllowMobileCheckIn { get; private set; }
    public bool AllowDesktopCheckIn { get; private set; }
    public bool AllowWebCheckIn { get; private set; }
    public bool AllowOfflineAttendance { get; private set; }
    public double AllowedRadiusMeters { get; private set; }
    public bool AllowMultipleCheckIn { get; private set; }
    public bool AllowManualAttendance { get; private set; }
    public bool RequireManagerApproval { get; private set; }
    public double MaximumDistanceMeters { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    // New Rules
    public int LateToleranceMinutes { get; private set; }
    public int EarlyLeaveToleranceMinutes { get; private set; }
    public int GracePeriodMinutes { get; private set; }
    public int OvertimeThresholdMinutes { get; private set; }
    public int MinimumOvertimeMinutes { get; private set; }
    public int MaximumOvertimeMinutes { get; private set; }
    
    public bool AutoCheckoutEnabled { get; private set; }
    public TimeOnly? AutoCheckoutTime { get; private set; }
    
    public bool MultipleShiftsSupport { get; private set; }
    
    public RoundTimeRule RoundTimeRule { get; private set; }
    public MissingPunchPolicy MissingPunchPolicy { get; private set; }
    public WeekendPolicy WeekendPolicy { get; private set; }
    public HolidayPolicy HolidayPolicy { get; private set; }

    // Relationships
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private AttendancePolicy() { }
#pragma warning restore CS8618

    private AttendancePolicy(
        Guid companyId,
        string name,
        string code,
        string? description,
        AttendanceMode attendanceMode,
        bool requireFaceRecognition,
        bool requireGPS,
        bool requireApprovedDevice,
        bool requireCompanyNetwork,
        bool allowMobileCheckIn,
        bool allowDesktopCheckIn,
        bool allowWebCheckIn,
        bool allowOfflineAttendance,
        double allowedRadiusMeters,
        bool allowMultipleCheckIn,
        bool allowManualAttendance,
        bool requireManagerApproval,
        double maximumDistanceMeters,
        int lateToleranceMinutes = 0,
        int earlyLeaveToleranceMinutes = 0,
        int gracePeriodMinutes = 0,
        int overtimeThresholdMinutes = 0,
        int minimumOvertimeMinutes = 0,
        int maximumOvertimeMinutes = 0,
        bool autoCheckoutEnabled = false,
        TimeOnly? autoCheckoutTime = null,
        bool multipleShiftsSupport = false,
        RoundTimeRule roundTimeRule = RoundTimeRule.None,
        MissingPunchPolicy missingPunchPolicy = MissingPunchPolicy.Reject,
        WeekendPolicy weekendPolicy = WeekendPolicy.Regular,
        HolidayPolicy holidayPolicy = HolidayPolicy.Regular)
    {
        CompanyId = companyId;
        Name = name;
        Code = code;
        Description = description;
        AttendanceMode = attendanceMode;
        RequireFaceRecognition = requireFaceRecognition;
        RequireGPS = requireGPS;
        RequireApprovedDevice = requireApprovedDevice;
        RequireCompanyNetwork = requireCompanyNetwork;
        AllowMobileCheckIn = allowMobileCheckIn;
        AllowDesktopCheckIn = allowDesktopCheckIn;
        AllowWebCheckIn = allowWebCheckIn;
        AllowOfflineAttendance = allowOfflineAttendance;
        AllowedRadiusMeters = allowedRadiusMeters;
        AllowMultipleCheckIn = allowMultipleCheckIn;
        AllowManualAttendance = allowManualAttendance;
        RequireManagerApproval = requireManagerApproval;
        MaximumDistanceMeters = maximumDistanceMeters;
        
        LateToleranceMinutes = lateToleranceMinutes;
        EarlyLeaveToleranceMinutes = earlyLeaveToleranceMinutes;
        GracePeriodMinutes = gracePeriodMinutes;
        OvertimeThresholdMinutes = overtimeThresholdMinutes;
        MinimumOvertimeMinutes = minimumOvertimeMinutes;
        MaximumOvertimeMinutes = maximumOvertimeMinutes;
        AutoCheckoutEnabled = autoCheckoutEnabled;
        AutoCheckoutTime = autoCheckoutTime;
        MultipleShiftsSupport = multipleShiftsSupport;
        RoundTimeRule = roundTimeRule;
        MissingPunchPolicy = missingPunchPolicy;
        WeekendPolicy = weekendPolicy;
        HolidayPolicy = holidayPolicy;
        
        IsDefault = false;
        IsActive = false; // Intentionally inactive upon creation
    }

    public static AttendancePolicy Create(
        Guid companyId,
        string name,
        string code,
        string? description,
        AttendanceMode attendanceMode,
        bool requireFaceRecognition,
        bool requireGPS,
        bool requireApprovedDevice,
        bool requireCompanyNetwork,
        bool allowMobileCheckIn,
        bool allowDesktopCheckIn,
        bool allowWebCheckIn,
        bool allowOfflineAttendance,
        double allowedRadiusMeters,
        bool allowMultipleCheckIn,
        bool allowManualAttendance,
        bool requireManagerApproval,
        double maximumDistanceMeters,
        int lateToleranceMinutes = 0,
        int earlyLeaveToleranceMinutes = 0,
        int gracePeriodMinutes = 0,
        int overtimeThresholdMinutes = 0,
        int minimumOvertimeMinutes = 0,
        int maximumOvertimeMinutes = 0,
        bool autoCheckoutEnabled = false,
        TimeOnly? autoCheckoutTime = null,
        bool multipleShiftsSupport = false,
        RoundTimeRule roundTimeRule = RoundTimeRule.None,
        MissingPunchPolicy missingPunchPolicy = MissingPunchPolicy.Reject,
        WeekendPolicy weekendPolicy = WeekendPolicy.Regular,
        HolidayPolicy holidayPolicy = HolidayPolicy.Regular)
    {
        if (companyId == Guid.Empty)
            throw new InvalidAttendancePolicyException("Company ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidAttendancePolicyException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidAttendancePolicyException("Code cannot be empty.");
        if (allowedRadiusMeters < 0)
            throw new InvalidAttendancePolicyException("Radius cannot be negative.");
        if (maximumDistanceMeters < 0)
            throw new InvalidAttendancePolicyException("Maximum distance meters cannot be negative.");

        // Mode-Specific Rule Validation
        if (attendanceMode == AttendanceMode.Field && !requireGPS)
            throw new InvalidAttendancePolicyException("Field mode requires GPS.");
        if (attendanceMode == AttendanceMode.Office && !requireFaceRecognition)
            throw new InvalidAttendancePolicyException("Office mode requires Face Recognition.");
        if (attendanceMode == AttendanceMode.Remote && requireCompanyNetwork)
            throw new InvalidAttendancePolicyException("Remote mode cannot require Company Network.");

        return new AttendancePolicy(
            companyId,
            name,
            code,
            description,
            attendanceMode,
            requireFaceRecognition,
            requireGPS,
            requireApprovedDevice,
            requireCompanyNetwork,
            allowMobileCheckIn,
            allowDesktopCheckIn,
            allowWebCheckIn,
            allowOfflineAttendance,
            allowedRadiusMeters,
            allowMultipleCheckIn,
            allowManualAttendance,
            requireManagerApproval,
            maximumDistanceMeters,
            lateToleranceMinutes,
            earlyLeaveToleranceMinutes,
            gracePeriodMinutes,
            overtimeThresholdMinutes,
            minimumOvertimeMinutes,
            maximumOvertimeMinutes,
            autoCheckoutEnabled,
            autoCheckoutTime,
            multipleShiftsSupport,
            roundTimeRule,
            missingPunchPolicy,
            weekendPolicy,
            holidayPolicy);
    }
    public void Update(
        string name,
        string code,
        string? description,
        AttendanceMode attendanceMode,
        bool requireFaceRecognition,
        bool requireGPS,
        bool requireApprovedDevice,
        bool requireCompanyNetwork,
        bool allowMobileCheckIn,
        bool allowDesktopCheckIn,
        bool allowWebCheckIn,
        bool allowOfflineAttendance,
        double allowedRadiusMeters,
        bool allowMultipleCheckIn,
        bool allowManualAttendance,
        bool requireManagerApproval,
        double maximumDistanceMeters,
        int lateToleranceMinutes,
        int earlyLeaveToleranceMinutes,
        int gracePeriodMinutes,
        int overtimeThresholdMinutes,
        int minimumOvertimeMinutes,
        int maximumOvertimeMinutes,
        bool autoCheckoutEnabled,
        TimeOnly? autoCheckoutTime,
        bool multipleShiftsSupport,
        RoundTimeRule roundTimeRule,
        MissingPunchPolicy missingPunchPolicy,
        WeekendPolicy weekendPolicy,
        HolidayPolicy holidayPolicy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidAttendancePolicyException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidAttendancePolicyException("Code cannot be empty.");
        if (allowedRadiusMeters < 0)
            throw new InvalidAttendancePolicyException("Radius cannot be negative.");
        if (maximumDistanceMeters < 0)
            throw new InvalidAttendancePolicyException("Maximum distance meters cannot be negative.");

        if (attendanceMode == AttendanceMode.Field && !requireGPS)
            throw new InvalidAttendancePolicyException("Field mode requires GPS.");
        if (attendanceMode == AttendanceMode.Office && !requireFaceRecognition)
            throw new InvalidAttendancePolicyException("Office mode requires Face Recognition.");
        if (attendanceMode == AttendanceMode.Remote && requireCompanyNetwork)
            throw new InvalidAttendancePolicyException("Remote mode cannot require Company Network.");

        Name = name;
        Code = code;
        Description = description;
        AttendanceMode = attendanceMode;
        RequireFaceRecognition = requireFaceRecognition;
        RequireGPS = requireGPS;
        RequireApprovedDevice = requireApprovedDevice;
        RequireCompanyNetwork = requireCompanyNetwork;
        AllowMobileCheckIn = allowMobileCheckIn;
        AllowDesktopCheckIn = allowDesktopCheckIn;
        AllowWebCheckIn = allowWebCheckIn;
        AllowOfflineAttendance = allowOfflineAttendance;
        AllowedRadiusMeters = allowedRadiusMeters;
        AllowMultipleCheckIn = allowMultipleCheckIn;
        AllowManualAttendance = allowManualAttendance;
        RequireManagerApproval = requireManagerApproval;
        MaximumDistanceMeters = maximumDistanceMeters;
        LateToleranceMinutes = lateToleranceMinutes;
        EarlyLeaveToleranceMinutes = earlyLeaveToleranceMinutes;
        GracePeriodMinutes = gracePeriodMinutes;
        OvertimeThresholdMinutes = overtimeThresholdMinutes;
        MinimumOvertimeMinutes = minimumOvertimeMinutes;
        MaximumOvertimeMinutes = maximumOvertimeMinutes;
        AutoCheckoutEnabled = autoCheckoutEnabled;
        AutoCheckoutTime = autoCheckoutTime;
        MultipleShiftsSupport = multipleShiftsSupport;
        RoundTimeRule = roundTimeRule;
        MissingPunchPolicy = missingPunchPolicy;
        WeekendPolicy = weekendPolicy;
        HolidayPolicy = holidayPolicy;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void EnableGPS()
    {
        RequireGPS = true;
    }

    public void DisableGPS()
    {
        if (AttendanceMode == AttendanceMode.Field)
            throw new InvalidAttendancePolicyException("Cannot disable GPS for Field mode.");
            
        RequireGPS = false;
    }

    public void EnableFaceRecognition()
    {
        RequireFaceRecognition = true;
    }

    public void DisableFaceRecognition()
    {
        if (AttendanceMode == AttendanceMode.Office)
            throw new InvalidAttendancePolicyException("Cannot disable Face Recognition for Office mode.");

        RequireFaceRecognition = false;
    }

    public void EnableOfflineAttendance()
    {
        AllowOfflineAttendance = true;
    }

    public void DisableOfflineAttendance()
    {
        AllowOfflineAttendance = false;
    }

    public void MarkAsDefault()
    {
        if (IsDefault) return;
        IsDefault = true;
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
    }
}

