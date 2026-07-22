using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class RestoreBreakCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class RestoreBreakCommandValidator : AbstractValidator<RestoreBreakCommand>
{
    public RestoreBreakCommandValidator()
    {
        RuleFor(v => v.BreakId).NotEmpty();
    }
}

public class RestoreBreakCommandHandler : IRequestHandler<RestoreBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreBreakCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        if (!breakEntity.IsDeleted)
            return Result.Failure(new[] { "Break is not deleted." });

        breakEntity.Restore();

        return Result.Success();
    }
}
