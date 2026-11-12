using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateDeviceSettingsCommand : IRequest<Result>
{
    public int DefaultDeviceTimeoutSeconds { get; set; }
    public int HeartbeatIntervalSeconds { get; set; }
    public bool AutoSyncEnabled { get; set; }
    public int RetryAttempts { get; set; }
    public int ConnectionTimeoutSeconds { get; set; }
}

public class UpdateDeviceSettingsCommandValidator : AbstractValidator<UpdateDeviceSettingsCommand>
{
    public UpdateDeviceSettingsCommandValidator()
    {
        RuleFor(x => x.DefaultDeviceTimeoutSeconds).GreaterThan(0);
        RuleFor(x => x.RetryAttempts).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDeviceSettingsCommandHandler : IRequestHandler<UpdateDeviceSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeviceSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateDeviceSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "Devices", new Dictionary<string, string>
        {
            { "DefaultDeviceTimeoutSeconds", request.DefaultDeviceTimeoutSeconds.ToString() },
            { "HeartbeatIntervalSeconds", request.HeartbeatIntervalSeconds.ToString() },
            { "AutoSyncEnabled", request.AutoSyncEnabled.ToString() },
            { "RetryAttempts", request.RetryAttempts.ToString() },
            { "ConnectionTimeoutSeconds", request.ConnectionTimeoutSeconds.ToString() },
        }, cancellationToken);

        return Result.Success();
    }
}
