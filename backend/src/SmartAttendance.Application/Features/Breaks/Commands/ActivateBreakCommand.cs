using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class ActivateBreakCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class ActivateBreakCommandValidator : AbstractValidator<ActivateBreakCommand>
{
    public ActivateBreakCommandValidator()
    {
        RuleFor(v => v.BreakId).NotEmpty();
    }
}

public class ActivateBreakCommandHandler : IRequestHandler<ActivateBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateBreakCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.Activate();

        return Result.Success();
    }
}
