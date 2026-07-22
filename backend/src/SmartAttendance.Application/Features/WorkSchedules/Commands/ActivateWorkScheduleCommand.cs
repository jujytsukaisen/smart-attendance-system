using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class ActivateWorkScheduleCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
}

public class ActivateWorkScheduleCommandValidator : AbstractValidator<ActivateWorkScheduleCommand>
{
    public ActivateWorkScheduleCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
    }
}

public class ActivateWorkScheduleCommandHandler : IRequestHandler<ActivateWorkScheduleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateWorkScheduleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .Include(w => w.Shifts) // Need shifts to validate activation rule
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
            return Result.Failure(new[] { "Work schedule not found." });

        schedule.Activate();

        return Result.Success();
    }
}

