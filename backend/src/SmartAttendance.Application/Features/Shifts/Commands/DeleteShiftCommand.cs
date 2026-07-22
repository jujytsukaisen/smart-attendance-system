using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class DeleteShiftCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
}

public class DeleteShiftCommandValidator : AbstractValidator<DeleteShiftCommand>
{
    public DeleteShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
    }
}

public class DeleteShiftCommandHandler : IRequestHandler<DeleteShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        shift.Delete();

        return Result.Success();
    }
}



