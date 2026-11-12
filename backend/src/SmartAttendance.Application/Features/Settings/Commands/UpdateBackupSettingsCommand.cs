using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateBackupSettingsCommand : IRequest<Result>
{
    public bool AutomaticBackupEnabled { get; set; }
    public string BackupTime { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public string StorageProvider { get; set; } = string.Empty;
}

public class UpdateBackupSettingsCommandValidator : AbstractValidator<UpdateBackupSettingsCommand>
{
    public UpdateBackupSettingsCommandValidator()
    {
        RuleFor(x => x.RetentionDays).GreaterThan(0);
    }
}

public class UpdateBackupSettingsCommandHandler : IRequestHandler<UpdateBackupSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateBackupSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateBackupSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "Backup", new Dictionary<string, string>
        {
            { "AutomaticBackupEnabled", request.AutomaticBackupEnabled.ToString() },
            { "BackupTime", request.BackupTime },
            { "RetentionDays", request.RetentionDays.ToString() },
            { "StorageProvider", request.StorageProvider },
        }, cancellationToken);

        return Result.Success();
    }
}
