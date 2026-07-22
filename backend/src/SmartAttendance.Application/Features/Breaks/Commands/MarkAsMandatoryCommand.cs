using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class MarkAsMandatoryCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class MarkAsMandatoryCommandValidator : AbstractValidator<MarkAsMandatoryCommand>
{
    public MarkAsMandatoryCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class MarkAsMandatoryCommandHandler : IRequestHandler<MarkAsMandatoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsMandatoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsMandatoryCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.MarkAsMandatory();

        return Result.Success();
    }
}

