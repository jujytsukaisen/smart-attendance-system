using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class RestoreShiftCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
}

public class RestoreShiftCommandValidator : AbstractValidator<RestoreShiftCommand>
{
    public RestoreShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
    }
}

public class RestoreShiftCommandHandler : IRequestHandler<RestoreShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        if (!shift.IsDeleted)
            return Result.Failure(new[] { "Shift is not deleted." });

        shift.Restore();

        return Result.Success();
    }
}
