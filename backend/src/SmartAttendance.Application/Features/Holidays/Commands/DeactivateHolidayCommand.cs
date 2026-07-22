using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Holidays.Commands;

public class DeactivateHolidayCommand : IRequest<Result>
{
    public Guid HolidayId { get; set; }
}

public class DeactivateHolidayCommandValidator : AbstractValidator<DeactivateHolidayCommand>
{
    public DeactivateHolidayCommandValidator()
    {
        RuleFor(x => x.HolidayId).NotEmpty();
    }
}

public class DeactivateHolidayCommandHandler : IRequestHandler<DeactivateHolidayCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == request.HolidayId, cancellationToken);
        if (holiday == null) return Result.Failure(new[] { "Holiday not found." });
        
        holiday.Deactivate();
        _context.Holidays.Update(holiday);

        return Result.Success();
    }
}

