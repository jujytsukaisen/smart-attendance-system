using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateAttendanceSettingsCommand : IRequest<Result>
{
    public string WorkingHoursStart { get; set; } = string.Empty;
    public string WorkingHoursEnd { get; set; } = string.Empty;
    public int GracePeriodMinutes { get; set; }
    public int LateThresholdMinutes { get; set; }
    public double MinimumHoursForHalfDay { get; set; }
    public double MinimumHoursForFullDay { get; set; }
    public double MaximumOvertimeHours { get; set; }
    public string WeekendRules { get; set; } = string.Empty;
    public string HolidayRules { get; set; } = string.Empty;
}

public class UpdateAttendanceSettingsCommandValidator : AbstractValidator<UpdateAttendanceSettingsCommand>
{
    public UpdateAttendanceSettingsCommandValidator()
    {
        RuleFor(x => x.WorkingHoursStart).NotEmpty();
        RuleFor(x => x.WorkingHoursEnd).NotEmpty();
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
    }
}

public class UpdateAttendanceSettingsCommandHandler : IRequestHandler<UpdateAttendanceSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAttendanceSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateAttendanceSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "Attendance", new Dictionary<string, string>
        {
            { "WorkingHoursStart", request.WorkingHoursStart },
            { "WorkingHoursEnd", request.WorkingHoursEnd },
            { "GracePeriodMinutes", request.GracePeriodMinutes.ToString() },
            { "LateThresholdMinutes", request.LateThresholdMinutes.ToString() },
            { "MinimumHoursForHalfDay", request.MinimumHoursForHalfDay.ToString() },
            { "MinimumHoursForFullDay", request.MinimumHoursForFullDay.ToString() },
            { "MaximumOvertimeHours", request.MaximumOvertimeHours.ToString() },
            { "WeekendRules", request.WeekendRules },
            { "HolidayRules", request.HolidayRules },
        }, cancellationToken);

        return Result.Success();
    }
}
