using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Settings.Commands;

namespace SmartAttendance.Application.Features.Settings.Queries;

// ──────────────────────────────────────────────────────────────
// DTOs (matching frontend types exactly)
// ──────────────────────────────────────────────────────────────

public record GeneralSettingsDto(
    string CompanyName,
    string DateFormat,
    string TimeFormat,
    string TimeZone,
    string Language,
    string Theme);

public record AttendanceSettingsDto(
    string WorkingHoursStart,
    string WorkingHoursEnd,
    int GracePeriodMinutes,
    int LateThresholdMinutes,
    double MinimumHoursForHalfDay,
    double MinimumHoursForFullDay,
    double MaximumOvertimeHours,
    string WeekendRules,
    string HolidayRules);

public record SecuritySettingsDto(
    string PasswordPolicy,
    int SessionTimeoutMinutes,
    int JwtLifetimeMinutes,
    int RefreshTokenLifetimeDays,
    bool TwoFactorAuthEnabled,
    int MaxLoginAttempts,
    int LockoutDurationMinutes);

public record NotificationSettingsDto(
    bool EmailNotificationsEnabled,
    bool SmsNotificationsEnabled,
    bool PushNotificationsEnabled,
    bool AttendanceAlertsEnabled,
    bool LeaveAlertsEnabled,
    bool DeviceAlertsEnabled);

public record DeviceSettingsDto(
    int DefaultDeviceTimeoutSeconds,
    int HeartbeatIntervalSeconds,
    bool AutoSyncEnabled,
    int RetryAttempts,
    int ConnectionTimeoutSeconds);

public record EmailSettingsDto(
    string SmtpHost,
    int SmtpPort,
    bool EnableSsl,
    string Username,
    string SenderName,
    string SenderEmail,
    string Password);

public record SmsSettingsDto(
    string Provider,
    string ApiKey,
    string SenderName);

public record BackupSettingsDto(
    bool AutomaticBackupEnabled,
    string BackupTime,
    int RetentionDays,
    string StorageProvider);

public record FaceRecognitionSettingsDto(
    double RecognitionThreshold,
    double MinimumFaceQuality,
    bool LivenessDetectionEnabled,
    bool MaskDetectionEnabled,
    bool SpoofDetectionEnabled);

public record SystemSettingsDto(
    GeneralSettingsDto General,
    AttendanceSettingsDto Attendance,
    SecuritySettingsDto Security,
    NotificationSettingsDto Notifications,
    DeviceSettingsDto Devices,
    EmailSettingsDto Email,
    SmsSettingsDto Sms,
    BackupSettingsDto Backup,
    FaceRecognitionSettingsDto FaceRecognition);

// ──────────────────────────────────────────────────────────────
// Queries
// ──────────────────────────────────────────────────────────────

public class GetGeneralSettingsQuery : IRequest<Result<GeneralSettingsDto>> { }

public class GetGeneralSettingsQueryHandler : IRequestHandler<GetGeneralSettingsQuery, Result<GeneralSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGeneralSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<GeneralSettingsDto>> Handle(GetGeneralSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "General", cancellationToken);
        return Result<GeneralSettingsDto>.Success(new GeneralSettingsDto(
            SettingsHelper.Get(d, "CompanyName", "My Company"),
            SettingsHelper.Get(d, "DateFormat", "DD/MM/YYYY"),
            SettingsHelper.Get(d, "TimeFormat", "HH:mm"),
            SettingsHelper.Get(d, "TimeZone", "UTC"),
            SettingsHelper.Get(d, "Language", "en"),
            SettingsHelper.Get(d, "Theme", "Light")));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetAttendanceSettingsQuery : IRequest<Result<AttendanceSettingsDto>> { }

public class GetAttendanceSettingsQueryHandler : IRequestHandler<GetAttendanceSettingsQuery, Result<AttendanceSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<AttendanceSettingsDto>> Handle(GetAttendanceSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "Attendance", cancellationToken);
        return Result<AttendanceSettingsDto>.Success(new AttendanceSettingsDto(
            SettingsHelper.Get(d, "WorkingHoursStart", "08:00"),
            SettingsHelper.Get(d, "WorkingHoursEnd", "17:00"),
            SettingsHelper.GetInt(d, "GracePeriodMinutes", 15),
            SettingsHelper.GetInt(d, "LateThresholdMinutes", 30),
            SettingsHelper.GetDouble(d, "MinimumHoursForHalfDay", 4),
            SettingsHelper.GetDouble(d, "MinimumHoursForFullDay", 8),
            SettingsHelper.GetDouble(d, "MaximumOvertimeHours", 3),
            SettingsHelper.Get(d, "WeekendRules", "Saturday,Sunday"),
            SettingsHelper.Get(d, "HolidayRules", "National")));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetSecuritySettingsQuery : IRequest<Result<SecuritySettingsDto>> { }

public class GetSecuritySettingsQueryHandler : IRequestHandler<GetSecuritySettingsQuery, Result<SecuritySettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSecuritySettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<SecuritySettingsDto>> Handle(GetSecuritySettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "Security", cancellationToken);
        return Result<SecuritySettingsDto>.Success(new SecuritySettingsDto(
            SettingsHelper.Get(d, "PasswordPolicy", "Strong"),
            SettingsHelper.GetInt(d, "SessionTimeoutMinutes", 60),
            SettingsHelper.GetInt(d, "JwtLifetimeMinutes", 60),
            SettingsHelper.GetInt(d, "RefreshTokenLifetimeDays", 7),
            SettingsHelper.GetBool(d, "TwoFactorAuthEnabled", false),
            SettingsHelper.GetInt(d, "MaxLoginAttempts", 5),
            SettingsHelper.GetInt(d, "LockoutDurationMinutes", 15)));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetNotificationSettingsQuery : IRequest<Result<NotificationSettingsDto>> { }

public class GetNotificationSettingsQueryHandler : IRequestHandler<GetNotificationSettingsQuery, Result<NotificationSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNotificationSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<NotificationSettingsDto>> Handle(GetNotificationSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "Notifications", cancellationToken);
        return Result<NotificationSettingsDto>.Success(new NotificationSettingsDto(
            SettingsHelper.GetBool(d, "EmailNotificationsEnabled", true),
            SettingsHelper.GetBool(d, "SmsNotificationsEnabled", false),
            SettingsHelper.GetBool(d, "PushNotificationsEnabled", true),
            SettingsHelper.GetBool(d, "AttendanceAlertsEnabled", true),
            SettingsHelper.GetBool(d, "LeaveAlertsEnabled", true),
            SettingsHelper.GetBool(d, "DeviceAlertsEnabled", true)));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetDeviceSettingsQuery : IRequest<Result<DeviceSettingsDto>> { }

public class GetDeviceSettingsQueryHandler : IRequestHandler<GetDeviceSettingsQuery, Result<DeviceSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<DeviceSettingsDto>> Handle(GetDeviceSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "Devices", cancellationToken);
        return Result<DeviceSettingsDto>.Success(new DeviceSettingsDto(
            SettingsHelper.GetInt(d, "DefaultDeviceTimeoutSeconds", 30),
            SettingsHelper.GetInt(d, "HeartbeatIntervalSeconds", 60),
            SettingsHelper.GetBool(d, "AutoSyncEnabled", true),
            SettingsHelper.GetInt(d, "RetryAttempts", 3),
            SettingsHelper.GetInt(d, "ConnectionTimeoutSeconds", 10)));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetEmailSettingsQuery : IRequest<Result<EmailSettingsDto>> { }

public class GetEmailSettingsQueryHandler : IRequestHandler<GetEmailSettingsQuery, Result<EmailSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmailSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<EmailSettingsDto>> Handle(GetEmailSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "Email", cancellationToken);
        return Result<EmailSettingsDto>.Success(new EmailSettingsDto(
            SettingsHelper.Get(d, "SmtpHost", "smtp.example.com"),
            SettingsHelper.GetInt(d, "SmtpPort", 587),
            SettingsHelper.GetBool(d, "EnableSsl", true),
            SettingsHelper.Get(d, "Username", ""),
            SettingsHelper.Get(d, "SenderName", "SmartAttendance"),
            SettingsHelper.Get(d, "SenderEmail", "noreply@smartattendance.local"),
            SettingsHelper.Get(d, "Password", "")
        ));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetSmsSettingsQuery : IRequest<Result<SmsSettingsDto>> { }

public class GetSmsSettingsQueryHandler : IRequestHandler<GetSmsSettingsQuery, Result<SmsSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSmsSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<SmsSettingsDto>> Handle(GetSmsSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "Sms", cancellationToken);
        return Result<SmsSettingsDto>.Success(new SmsSettingsDto(
            SettingsHelper.Get(d, "Provider", "Twilio"),
            SettingsHelper.Get(d, "ApiKey", ""),
            SettingsHelper.Get(d, "SenderName", "SmartAttendance")));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetBackupSettingsQuery : IRequest<Result<BackupSettingsDto>> { }

public class GetBackupSettingsQueryHandler : IRequestHandler<GetBackupSettingsQuery, Result<BackupSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBackupSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<BackupSettingsDto>> Handle(GetBackupSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "Backup", cancellationToken);
        return Result<BackupSettingsDto>.Success(new BackupSettingsDto(
            SettingsHelper.GetBool(d, "AutomaticBackupEnabled", false),
            SettingsHelper.Get(d, "BackupTime", "00:00"),
            SettingsHelper.GetInt(d, "RetentionDays", 30),
            SettingsHelper.Get(d, "StorageProvider", "Local")));
    }
}

// ──────────────────────────────────────────────────────────────

public class GetFaceRecognitionSettingsQuery : IRequest<Result<FaceRecognitionSettingsDto>> { }

public class GetFaceRecognitionSettingsQueryHandler : IRequestHandler<GetFaceRecognitionSettingsQuery, Result<FaceRecognitionSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFaceRecognitionSettingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<FaceRecognitionSettingsDto>> Handle(GetFaceRecognitionSettingsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        var d = await SettingsHelper.GetCategoryAsync(_context, companyId, "FaceRecognition", cancellationToken);

        var threshold = SettingsHelper.GetDouble(d, "RecognitionThreshold", 80.0);
        if (threshold > 0 && threshold <= 1.0) threshold *= 100.0;

        var quality = SettingsHelper.GetDouble(d, "MinimumFaceQuality", 60.0);
        if (quality > 0 && quality <= 1.0) quality *= 100.0;

        return Result<FaceRecognitionSettingsDto>.Success(new FaceRecognitionSettingsDto(
            threshold,
            quality,
            SettingsHelper.GetBool(d, "LivenessDetectionEnabled", true),
            SettingsHelper.GetBool(d, "MaskDetectionEnabled", false),
            SettingsHelper.GetBool(d, "SpoofDetectionEnabled", true)));
    }
}

// ──────────────────────────────────────────────────────────────
// System Settings (aggregate all categories)
// ──────────────────────────────────────────────────────────────

public class GetSystemSettingsQuery : IRequest<Result<SystemSettingsDto>> { }

public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, Result<SystemSettingsDto>>
{
    private readonly ISender _mediator;

    public GetSystemSettingsQueryHandler(ISender mediator) => _mediator = mediator;

    public async Task<Result<SystemSettingsDto>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        var general = await _mediator.Send(new GetGeneralSettingsQuery(), cancellationToken);
        var attendance = await _mediator.Send(new GetAttendanceSettingsQuery(), cancellationToken);
        var security = await _mediator.Send(new GetSecuritySettingsQuery(), cancellationToken);
        var notifications = await _mediator.Send(new GetNotificationSettingsQuery(), cancellationToken);
        var devices = await _mediator.Send(new GetDeviceSettingsQuery(), cancellationToken);
        var email = await _mediator.Send(new GetEmailSettingsQuery(), cancellationToken);
        var sms = await _mediator.Send(new GetSmsSettingsQuery(), cancellationToken);
        var backup = await _mediator.Send(new GetBackupSettingsQuery(), cancellationToken);
        var faceRecognition = await _mediator.Send(new GetFaceRecognitionSettingsQuery(), cancellationToken);

        return Result<SystemSettingsDto>.Success(new SystemSettingsDto(
            general.Data!,
            attendance.Data!,
            security.Data!,
            notifications.Data!,
            devices.Data!,
            email.Data!,
            sms.Data!,
            backup.Data!,
            faceRecognition.Data!));
    }
}
