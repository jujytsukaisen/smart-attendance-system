using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

// ──────────────────────────────────────────────────────────────
// Reset a single section
// ──────────────────────────────────────────────────────────────

public class ResetSectionSettingsCommand : IRequest<Result>
{
    public string Section { get; set; } = string.Empty;
}

public class ResetSectionSettingsCommandHandler : IRequestHandler<ResetSectionSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Maps the camelCase section name (from the frontend URL) to the DB Category string.
    /// </summary>
    private static readonly Dictionary<string, string> SectionToCategory = new(StringComparer.OrdinalIgnoreCase)
    {
        { "general",          "General" },
        { "attendance",       "Attendance" },
        { "security",         "Security" },
        { "notifications",    "Notifications" },
        { "devices",          "Devices" },
        { "email",            "Email" },
        { "sms",              "Sms" },
        { "backup",           "Backup" },
        { "faceRecognition",  "FaceRecognition" },
        { "face-recognition", "FaceRecognition" },
    };

    public ResetSectionSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ResetSectionSettingsCommand request, CancellationToken cancellationToken)
    {
        if (!SectionToCategory.TryGetValue(request.Section, out var category))
            return Result.Failure(new[] { $"Unknown settings section: '{request.Section}'." });

        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty)
            return Result.Failure(new[] { "Unable to determine company context." });

        // Load then remove — avoids EF Core tracking issues with ExecuteDelete on older providers
        var rows = await _context.AppSettings
            .Where(s => s.CompanyId == companyId && s.Category == category)
            .ToListAsync(cancellationToken);

        if (rows.Count > 0)
        {
            _context.AppSettings.RemoveRange(rows);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // After deletion the next GET will return the hardcoded defaults from the query handlers.
        return Result.Success();
    }
}

// ──────────────────────────────────────────────────────────────
// Reset ALL sections
// ──────────────────────────────────────────────────────────────

public class ResetAllSettingsCommand : IRequest<Result> { }

public class ResetAllSettingsCommandHandler : IRequestHandler<ResetAllSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ResetAllSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ResetAllSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty)
            return Result.Failure(new[] { "Unable to determine company context." });

        var rows = await _context.AppSettings
            .Where(s => s.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        if (rows.Count > 0)
        {
            _context.AppSettings.RemoveRange(rows);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
