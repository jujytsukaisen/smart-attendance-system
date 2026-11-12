using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateNotificationSettingsCommand : IRequest<Result>
{
    public bool EmailNotificationsEnabled { get; set; }
    public bool SmsNotificationsEnabled { get; set; }
    public bool PushNotificationsEnabled { get; set; }
    public bool AttendanceAlertsEnabled { get; set; }
    public bool LeaveAlertsEnabled { get; set; }
    public bool DeviceAlertsEnabled { get; set; }
}

public class UpdateNotificationSettingsCommandValidator : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator() { }
}

public class UpdateNotificationSettingsCommandHandler : IRequestHandler<UpdateNotificationSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateNotificationSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "Notifications", new Dictionary<string, string>
        {
            { "EmailNotificationsEnabled", request.EmailNotificationsEnabled.ToString() },
            { "SmsNotificationsEnabled", request.SmsNotificationsEnabled.ToString() },
            { "PushNotificationsEnabled", request.PushNotificationsEnabled.ToString() },
            { "AttendanceAlertsEnabled", request.AttendanceAlertsEnabled.ToString() },
            { "LeaveAlertsEnabled", request.LeaveAlertsEnabled.ToString() },
            { "DeviceAlertsEnabled", request.DeviceAlertsEnabled.ToString() },
        }, cancellationToken);

        return Result.Success();
    }
}
