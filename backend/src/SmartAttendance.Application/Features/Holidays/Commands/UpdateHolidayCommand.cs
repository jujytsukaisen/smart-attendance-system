using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Holidays.Commands;

public class UpdateHolidayCommand : IRequest<Result>
{
    public Guid HolidayId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateHolidayCommandValidator : AbstractValidator<UpdateHolidayCommand>
{
    public UpdateHolidayCommandValidator()
    {
        RuleFor(x => x.HolidayId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HolidayDate).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateHolidayCommandHandler : IRequestHandler<UpdateHolidayCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == request.HolidayId, cancellationToken);
        if (holiday == null) return Result.Failure(new[] { "Holiday not found." });
        
        holiday.Rename(request.Name, request.Description);
        holiday.ChangeDates(request.HolidayDate, request.HolidayDate); // Assuming single day
        
        _context.Holidays.Update(holiday);

        return Result.Success();
    }
}



