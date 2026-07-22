using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class AddBreakCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public Guid BreakId { get; set; }
}

public class AddBreakCommandValidator : AbstractValidator<AddBreakCommand>
{
    public AddBreakCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class AddBreakCommandHandler : IRequestHandler<AddBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AddBreakCommand request, CancellationToken cancellationToken)
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

        shift.AddBreak(breakEntity);

        return Result.Success();
    }
}

