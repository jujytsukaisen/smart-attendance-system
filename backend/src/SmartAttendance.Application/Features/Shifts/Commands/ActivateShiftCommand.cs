using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class ActivateShiftCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
}

public class ActivateShiftCommandValidator : AbstractValidator<ActivateShiftCommand>
{
    public ActivateShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
    }
}

public class ActivateShiftCommandHandler : IRequestHandler<ActivateShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        shift.Activate();

        return Result.Success();
    }
}

