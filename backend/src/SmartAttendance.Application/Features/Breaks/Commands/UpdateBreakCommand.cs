using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class UpdateBreakCommand : IRequest<Result>
{
    public Guid BreakId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public SmartAttendance.Domain.Enums.BreakType BreakType { get; set; }
    public bool IsPaid { get; set; }
    public bool IsMandatory { get; set; }
}

public class UpdateBreakCommandValidator : AbstractValidator<UpdateBreakCommand>
{
    public UpdateBreakCommandValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
    }
}

public class UpdateBreakCommandHandler : IRequestHandler<UpdateBreakCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateBreakCommand request, CancellationToken cancellationToken)
    {
        var breakEntity = await _context.Breaks
            .FirstOrDefaultAsync(b => b.Id == request.BreakId, cancellationToken);

        if (breakEntity == null)
            return Result.Failure(new[] { "Break not found." });

        breakEntity.Rename(request.Name);
        breakEntity.ChangeDescription(request.Description);
        breakEntity.ChangeTime(request.StartTime, request.EndTime, request.DurationMinutes);
        
        if (request.IsPaid) breakEntity.MarkAsPaid();
        else breakEntity.MarkAsUnpaid();
        
        if (request.IsMandatory) breakEntity.MarkAsMandatory();
        else breakEntity.MarkAsOptional();

        return Result.Success();
    }
}



