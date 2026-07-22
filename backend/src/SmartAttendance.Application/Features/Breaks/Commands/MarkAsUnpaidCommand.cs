using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class MarkAsUnpaidCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class MarkAsUnpaidCommandValidator : AbstractValidator<MarkAsUnpaidCommand>
{
    public MarkAsUnpaidCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class MarkAsUnpaidCommandHandler : IRequestHandler<MarkAsUnpaidCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsUnpaidCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsUnpaidCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.MarkAsUnpaid();

        return Result.Success();
    }
}

