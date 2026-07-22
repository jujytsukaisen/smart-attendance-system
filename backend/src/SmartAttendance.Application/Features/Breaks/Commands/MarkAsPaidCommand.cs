using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class MarkAsPaidCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class MarkAsPaidCommandValidator : AbstractValidator<MarkAsPaidCommand>
{
    public MarkAsPaidCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class MarkAsPaidCommandHandler : IRequestHandler<MarkAsPaidCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsPaidCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsPaidCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.MarkAsPaid();

        return Result.Success();
    }
}

