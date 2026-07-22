using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Holidays.Commands;

public class DeleteHolidayCommand : IRequest<Result>
{
    public Guid HolidayId { get; set; }
}

public class DeleteHolidayCommandValidator : AbstractValidator<DeleteHolidayCommand>
{
    public DeleteHolidayCommandValidator()
    {
        RuleFor(x => x.HolidayId).NotEmpty();
    }
}

public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == request.HolidayId, cancellationToken);
        if (holiday == null) return Result.Failure(new[] { "Holiday not found." });
        
        _context.Holidays.Remove(holiday);

        return Result.Success();
    }
}



