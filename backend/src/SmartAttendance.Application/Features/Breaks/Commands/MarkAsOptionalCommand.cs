using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class MarkAsOptionalCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class MarkAsOptionalCommandValidator : AbstractValidator<MarkAsOptionalCommand>
{
    public MarkAsOptionalCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class MarkAsOptionalCommandHandler : IRequestHandler<MarkAsOptionalCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsOptionalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsOptionalCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.MarkAsOptional();

        return Result.Success();
    }
}

