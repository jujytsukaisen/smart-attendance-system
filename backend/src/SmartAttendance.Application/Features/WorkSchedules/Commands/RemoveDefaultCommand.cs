using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class RemoveDefaultCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
}

public class RemoveDefaultCommandValidator : AbstractValidator<RemoveDefaultCommand>
{
    public RemoveDefaultCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
    }
}

public class RemoveDefaultCommandHandler : IRequestHandler<RemoveDefaultCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveDefaultCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveDefaultCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
            return Result.Failure(new[] { "Work schedule not found." });

        schedule.RemoveDefault();

        return Result.Success();
    }
}

