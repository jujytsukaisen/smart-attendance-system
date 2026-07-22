using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class RestoreWorkScheduleCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
}

public class RestoreWorkScheduleCommandValidator : AbstractValidator<RestoreWorkScheduleCommand>
{
    public RestoreWorkScheduleCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
    }
}

public class RestoreWorkScheduleCommandHandler : IRequestHandler<RestoreWorkScheduleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreWorkScheduleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
            return Result.Failure(new[] { "Work schedule not found." });

        if (!schedule.IsDeleted)
            return Result.Failure(new[] { "Work schedule is not deleted." });

        schedule.Restore();

        return Result.Success();
    }
}
