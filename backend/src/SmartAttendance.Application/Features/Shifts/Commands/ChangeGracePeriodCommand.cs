using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class ChangeGracePeriodCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public int GracePeriodMinutes { get; set; }
}

public class ChangeGracePeriodCommandValidator : AbstractValidator<ChangeGracePeriodCommand>
{
    public ChangeGracePeriodCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
    }
}

public class ChangeGracePeriodCommandHandler : IRequestHandler<ChangeGracePeriodCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeGracePeriodCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeGracePeriodCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        shift.ChangeGracePeriod(request.GracePeriodMinutes);

        return Result.Success();
    }
}

