using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateSecuritySettingsCommand : IRequest<Result>
{
    public string PasswordPolicy { get; set; } = string.Empty;
    public int SessionTimeoutMinutes { get; set; }
    public int JwtLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeDays { get; set; }
    public bool TwoFactorAuthEnabled { get; set; }
    public int MaxLoginAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
}

public class UpdateSecuritySettingsCommandValidator : AbstractValidator<UpdateSecuritySettingsCommand>
{
    public UpdateSecuritySettingsCommandValidator()
    {
        RuleFor(x => x.SessionTimeoutMinutes).GreaterThan(0);
        RuleFor(x => x.MaxLoginAttempts).GreaterThan(0);
    }
}

public class UpdateSecuritySettingsCommandHandler : IRequestHandler<UpdateSecuritySettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateSecuritySettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateSecuritySettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "Security", new Dictionary<string, string>
        {
            { "PasswordPolicy", request.PasswordPolicy },
            { "SessionTimeoutMinutes", request.SessionTimeoutMinutes.ToString() },
            { "JwtLifetimeMinutes", request.JwtLifetimeMinutes.ToString() },
            { "RefreshTokenLifetimeDays", request.RefreshTokenLifetimeDays.ToString() },
            { "TwoFactorAuthEnabled", request.TwoFactorAuthEnabled.ToString() },
            { "MaxLoginAttempts", request.MaxLoginAttempts.ToString() },
            { "LockoutDurationMinutes", request.LockoutDurationMinutes.ToString() },
        }, cancellationToken);

        return Result.Success();
    }
}
