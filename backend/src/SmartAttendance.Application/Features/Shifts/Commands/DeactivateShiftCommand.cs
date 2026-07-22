using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class DeactivateShiftCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
}

public class DeactivateShiftCommandValidator : AbstractValidator<DeactivateShiftCommand>
{
    public DeactivateShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
    }
}

public class DeactivateShiftCommandHandler : IRequestHandler<DeactivateShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        shift.Deactivate();

        return Result.Success();
    }
}

