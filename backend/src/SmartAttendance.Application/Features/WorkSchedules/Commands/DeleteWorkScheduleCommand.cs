using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class DeleteWorkScheduleCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
}

public class DeleteWorkScheduleCommandValidator : AbstractValidator<DeleteWorkScheduleCommand>
{
    public DeleteWorkScheduleCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
    }
}

public class DeleteWorkScheduleCommandHandler : IRequestHandler<DeleteWorkScheduleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkScheduleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
            return Result.Failure(new[] { "Work schedule not found." });

        schedule.Delete();

        return Result.Success();
    }
}



