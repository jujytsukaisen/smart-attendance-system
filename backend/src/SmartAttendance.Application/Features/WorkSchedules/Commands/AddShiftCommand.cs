using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class AddShiftCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
    public Guid ShiftId { get; set; }
}

public class AddShiftCommandValidator : AbstractValidator<AddShiftCommand>
{
    public AddShiftCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
        RuleFor(x => x.ShiftId).NotEmpty();
    }
}

public class AddShiftCommandHandler : IRequestHandler<AddShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AddShiftCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .Include(w => w.Shifts)
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
            return Result.Failure(new[] { "Work schedule not found." });

        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        schedule.AddShift(shift);

        return Result.Success();
    }
}

