using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Shifts.Commands;

public class UpdateShiftCommand : IRequest<Result>
{
    public Guid ShiftId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
{
    public UpdateShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result.Failure(new[] { "Shift not found." });

        var codeExists = await _context.Shifts
            .AnyAsync(s => s.CompanyId == shift.CompanyId && s.Code == request.Code && s.Id != shift.Id, cancellationToken);

        if (codeExists)
            return Result.Failure(new[] { "A shift with this code already exists." });

        shift.Update(request.Name, request.Code, request.Description);

        return Result.Success();
    }
}



