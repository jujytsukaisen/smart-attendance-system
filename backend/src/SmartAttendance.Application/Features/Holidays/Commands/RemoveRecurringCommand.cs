using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Holidays.Commands;

public class RemoveRecurringCommand : IRequest<Result>
{
    public Guid HolidayId { get; set; }
}

public class RemoveRecurringCommandValidator : AbstractValidator<RemoveRecurringCommand>
{
    public RemoveRecurringCommandValidator()
    {
        RuleFor(x => x.HolidayId).NotEmpty();
    }
}

public class RemoveRecurringCommandHandler : IRequestHandler<RemoveRecurringCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveRecurringCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveRecurringCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == request.HolidayId, cancellationToken);
        if (holiday == null) return Result.Failure(new[] { "Holiday not found." });
        
        holiday.ChangeDates(holiday.StartDate, holiday.EndDate, false);
        _context.Holidays.Update(holiday);

        return Result.Success();
    }
}

