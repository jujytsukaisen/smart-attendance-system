using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class ChangeTimeCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DurationMinutes { get; set; }
}

public class ChangeTimeCommandValidator : AbstractValidator<ChangeTimeCommand>
{
    public ChangeTimeCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
    }
}

public class ChangeTimeCommandHandler : IRequestHandler<ChangeTimeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeTimeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeTimeCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.ChangeTime(request.StartTime, request.EndTime, request.DurationMinutes);

        return Result.Success();
    }
}

