using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class RenameBreakCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RenameBreakCommandValidator : AbstractValidator<RenameBreakCommand>
{
    public RenameBreakCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class RenameBreakCommandHandler : IRequestHandler<RenameBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RenameBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RenameBreakCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.Rename(request.Name);

        return Result.Success();
    }
}

