using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class ChangeWorkingHoursCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public double MinimumWorkingHours { get; set; }
    public double MaximumWorkingHours { get; set; }
}

public class ChangeWorkingHoursCommandValidator : AbstractValidator<ChangeWorkingHoursCommand>
{
    public ChangeWorkingHoursCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x.MinimumWorkingHours).GreaterThan(0);
        RuleFor(x => x.MaximumWorkingHours)
            .GreaterThanOrEqualTo(x => x.MinimumWorkingHours)
            .WithMessage("Maximum working hours must be greater than or equal to minimum working hours.");
    }
}

public class ChangeWorkingHoursCommandHandler : IRequestHandler<ChangeWorkingHoursCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeWorkingHoursCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeWorkingHoursCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        shift.ChangeWorkingHours(request.StartTime, request.EndTime, request.MinimumWorkingHours, request.MaximumWorkingHours);

        return Result.Success();
    }
}

