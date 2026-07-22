using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Holidays.Commands;

public class CreateHolidayCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
}

public class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HolidayDate).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = SmartAttendance.Domain.Entities.Holiday.Create(
            request.CompanyId,
            request.Name,
            request.Description,
            SmartAttendance.Domain.Enums.HolidayType.PublicHoliday, // Defaulting to public since not in command
            request.HolidayDate,
            request.HolidayDate, // Assuming single day if not provided
            request.IsRecurring,
            null);
            
        await _context.Holidays.AddAsync(holiday, cancellationToken);

        return Result<Guid>.Success(holiday.Id);
    }
}

