using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class DeactivateBreakCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class DeactivateBreakCommandValidator : AbstractValidator<DeactivateBreakCommand>
{
    public DeactivateBreakCommandValidator()
    {
        RuleFor(v => v.BreakId).NotEmpty();
    }
}

public class DeactivateBreakCommandHandler : IRequestHandler<DeactivateBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateBreakCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.Deactivate();

        return Result.Success();
    }
}
