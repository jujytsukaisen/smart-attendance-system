using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class UpdateShiftCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int ShiftType { get; set; }
    public bool IsFlexible { get; set; }
    public double MinimumWorkingHours { get; set; }
    public double MaximumWorkingHours { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int AllowEarlyCheckInMinutes { get; set; }
    public int AllowLateCheckOutMinutes { get; set; }
    public int MaxBreakDurationMinutes { get; set; }
}

public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
{
    public UpdateShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.StartTime);
        RuleFor(x => x.EndTime);
        RuleFor(x => x.ShiftType).InclusiveBetween(1, 5);
        RuleFor(x => x.MinimumWorkingHours).GreaterThan(0);
        RuleFor(x => x.MaximumWorkingHours).GreaterThanOrEqualTo(x => x.MinimumWorkingHours);
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowEarlyCheckInMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowLateCheckOutMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxBreakDurationMinutes).GreaterThanOrEqualTo(0);
    }
}

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        var codeExists = await _context.Shifts
            .AnyAsync(s => s.CompanyId == shift.CompanyId && s.Code == request.Code && s.Id != shift.Id, cancellationToken);

        if (codeExists)
            return Result.Failure(new[] { "A shift with this code already exists." });

        shift.Update(request.Name, request.Code, request.Description);

        if (!TimeOnly.TryParse(request.StartTime, out var startTime))
            return Result.Failure(new[] { "Invalid StartTime format." });
        if (!TimeOnly.TryParse(request.EndTime, out var endTime))
            return Result.Failure(new[] { "Invalid EndTime format." });

        shift.ChangeWorkingHours(startTime, endTime, request.MinimumWorkingHours, request.MaximumWorkingHours);
        shift.ChangeGracePeriod(request.GracePeriodMinutes);
        shift.ChangeCheckInRules(request.AllowEarlyCheckInMinutes, request.AllowLateCheckOutMinutes);
        shift.ChangeMaxBreakDuration(request.MaxBreakDurationMinutes);
        shift.ChangeTypeAndFlexibility((SmartAttendance.Domain.Enums.ShiftType)request.ShiftType, request.IsFlexible);

        return Result.Success();
    }
}



