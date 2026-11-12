using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Settings.Commands;

// ──────────────────────────────────────────────────────────────
// General Settings
// ──────────────────────────────────────────────────────────────

public class UpdateGeneralSettingsCommand : IRequest<Result>
{
    public string CompanyName { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
    public string TimeFormat { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
}

public class UpdateGeneralSettingsCommandValidator : AbstractValidator<UpdateGeneralSettingsCommand>
{
    public UpdateGeneralSettingsCommandValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TimeZone).NotEmpty();
        RuleFor(x => x.DateFormat).NotEmpty();
        RuleFor(x => x.TimeFormat).NotEmpty();
        RuleFor(x => x.Language).NotEmpty();
        RuleFor(x => x.Theme).NotEmpty();
    }
}

public class UpdateGeneralSettingsCommandHandler : IRequestHandler<UpdateGeneralSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateGeneralSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateGeneralSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty)
            return Result.Failure(new[] { "Unable to determine company context." });

        var values = new Dictionary<string, string>
        {
            { "CompanyName", request.CompanyName },
            { "DateFormat", request.DateFormat },
            { "TimeFormat", request.TimeFormat },
            { "TimeZone", request.TimeZone },
            { "Language", request.Language },
            { "Theme", request.Theme },
        };

        await SettingsHelper.UpsertAsync(_context, companyId, "General", values, cancellationToken);
        return Result.Success();
    }
}
