using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class CreateShiftCommand : IRequest<Result<Guid>>
{
    public Guid WorkScheduleId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public bool IsFlexible { get; set; }
    public double MinimumWorkingHours { get; set; }
    public double MaximumWorkingHours { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int AllowEarlyCheckInMinutes { get; set; }
    public int AllowLateCheckOutMinutes { get; set; }
}

public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x.MinimumWorkingHours).GreaterThan(0);
        RuleFor(x => x.MaximumWorkingHours)
            .GreaterThanOrEqualTo(x => x.MinimumWorkingHours)
            .WithMessage("Maximum working hours must be greater than or equal to minimum working hours.");
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowEarlyCheckInMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowLateCheckOutMinutes).GreaterThanOrEqualTo(0);
    }
}

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .Include(w => w.Shifts)
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId && w.CompanyId == request.CompanyId, cancellationToken);

        if (schedule == null)
        {
            return Result<Guid>.Failure(new[] { "Work schedule not found or does not belong to the company." });
        }

        var codeExists = await _context.Shifts
            .AnyAsync(s => s.CompanyId == request.CompanyId && s.Code == request.Code, cancellationToken);
        if (codeExists)
        {
            return Result<Guid>.Failure(new[] { "A shift with this code already exists." });
        }

        var shift = Shift.Create(
            request.CompanyId,
            request.WorkScheduleId,
            request.Name,
            request.Code,
            request.Description,
            request.StartTime,
            request.EndTime,
            request.ShiftType,
            request.IsFlexible,
            request.MinimumWorkingHours,
            request.MaximumWorkingHours,
            request.GracePeriodMinutes,
            request.AllowEarlyCheckInMinutes,
            request.AllowLateCheckOutMinutes);

        schedule.AddShift(shift);

        await _context.Shifts.AddAsync(shift, cancellationToken);

        return Result<Guid>.Success(shift.Id);
    }
}

