using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class MarkAsDefaultCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
}

public class MarkAsDefaultCommandValidator : AbstractValidator<MarkAsDefaultCommand>
{
    public MarkAsDefaultCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
    }
}

public class MarkAsDefaultCommandHandler : IRequestHandler<MarkAsDefaultCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsDefaultCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsDefaultCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
            return Result.Failure(new[] { "Work schedule not found." });

        // Domain Rule: Only one default per company. Remove default from others.
        var currentDefaults = await _context.WorkSchedules
            .Where(w => w.CompanyId == schedule.CompanyId && w.IsDefault && w.Id != schedule.Id)
            .ToListAsync(cancellationToken);

        foreach (var def in currentDefaults)
        {
            def.RemoveDefault();
        }

        schedule.MarkAsDefault();

        return Result.Success();
    }
}

