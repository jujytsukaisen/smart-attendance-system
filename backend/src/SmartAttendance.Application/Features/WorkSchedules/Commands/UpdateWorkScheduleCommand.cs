using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class UpdateWorkScheduleCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFlexible { get; set; }
    public double WeeklyWorkingHours { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int AllowEarlyCheckInMinutes { get; set; }
    public int AllowLateCheckOutMinutes { get; set; }
}

public class UpdateWorkScheduleCommandValidator : AbstractValidator<UpdateWorkScheduleCommand>
{
    public UpdateWorkScheduleCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.WeeklyWorkingHours).GreaterThan(0);
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowEarlyCheckInMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowLateCheckOutMinutes).GreaterThanOrEqualTo(0);
    }
}

public class UpdateWorkScheduleCommandHandler : IRequestHandler<UpdateWorkScheduleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkScheduleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
        {
            return Result.Failure(new[] { "Work schedule not found." });
        }

        var nameExists = await _context.WorkSchedules
            .AnyAsync(w => w.CompanyId == schedule.CompanyId && w.Name == request.Name && w.Id != schedule.Id, cancellationToken);

        if (nameExists)
        {
            return Result.Failure(new[] { "A work schedule with this name already exists." });
        }

        schedule.Update(
            request.Name,
            request.Description,
            request.IsFlexible,
            request.WeeklyWorkingHours,
            request.GracePeriodMinutes,
            request.AllowEarlyCheckInMinutes,
            request.AllowLateCheckOutMinutes);

        return Result.Success();
    }
}



