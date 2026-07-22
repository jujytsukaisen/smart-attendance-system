using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class RemoveBreakCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public Guid BreakId { get; set; }
}

public class RemoveBreakCommandValidator : AbstractValidator<RemoveBreakCommand>
{
    public RemoveBreakCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class RemoveBreakCommandHandler : IRequestHandler<RemoveBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveBreakCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .Include(s => s.Breaks)
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        shift.RemoveBreak(breakEntity);

        return Result.Success();
    }
}

