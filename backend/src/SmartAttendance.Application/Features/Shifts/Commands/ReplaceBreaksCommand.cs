using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class ReplaceBreaksCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public List<Guid> BreakIds { get; set; } = new();
}

public class ReplaceBreaksCommandValidator : AbstractValidator<ReplaceBreaksCommand>
{
    public ReplaceBreaksCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.BreakIds).NotEmpty();
    }
}

public class ReplaceBreaksCommandHandler : IRequestHandler<ReplaceBreaksCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReplaceBreaksCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ReplaceBreaksCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .Include(s => s.Breaks)
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        var breaks = await _context.Breaks
            .Where(b => request.BreakIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        if (breaks.Count != request.BreakIds.Count)
        {
            return Result.Failure(new[] { "One or more breaks not found." });
        }

        shift.ReplaceBreaks(breaks);

        return Result.Success();
    }
}

