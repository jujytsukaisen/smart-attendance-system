using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Holidays.Commands;

public class ActivateHolidayCommand : IRequest<Result>
{
    public Guid HolidayId { get; set; }
}

public class ActivateHolidayCommandValidator : AbstractValidator<ActivateHolidayCommand>
{
    public ActivateHolidayCommandValidator()
    {
        RuleFor(x => x.HolidayId).NotEmpty();
    }
}

public class ActivateHolidayCommandHandler : IRequestHandler<ActivateHolidayCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == request.HolidayId, cancellationToken);
        if (holiday == null) return Result.Failure(new[] { "Holiday not found." });
        
        holiday.Activate();
        _context.Holidays.Update(holiday);

        return Result.Success();
    }
}

