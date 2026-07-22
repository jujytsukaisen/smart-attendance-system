using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class DeleteBreakCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
}

public class DeleteBreakCommandValidator : AbstractValidator<DeleteBreakCommand>
{
    public DeleteBreakCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class DeleteBreakCommandHandler : IRequestHandler<DeleteBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteBreakCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.Delete();

        return Result.Success();
    }
}



