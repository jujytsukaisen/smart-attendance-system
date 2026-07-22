using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class ChangeCheckInRulesCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public int AllowEarlyCheckInMinutes { get; set; }
    public int AllowLateCheckOutMinutes { get; set; }
}

public class ChangeCheckInRulesCommandValidator : AbstractValidator<ChangeCheckInRulesCommand>
{
    public ChangeCheckInRulesCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.AllowEarlyCheckInMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowLateCheckOutMinutes).GreaterThanOrEqualTo(0);
    }
}

public class ChangeCheckInRulesCommandHandler : IRequestHandler<ChangeCheckInRulesCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeCheckInRulesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeCheckInRulesCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        shift.ChangeCheckInRules(request.AllowEarlyCheckInMinutes, request.AllowLateCheckOutMinutes);

        return Result.Success();
    }
}

