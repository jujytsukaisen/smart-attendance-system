using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class ChangeRadiusCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
    public double AllowedRadiusMeters { get; set; }
}

public class ChangeRadiusCommandValidator : AbstractValidator<ChangeRadiusCommand>
{
    public ChangeRadiusCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.AllowedRadiusMeters).GreaterThan(0);
    }
}

public class ChangeRadiusCommandHandler : IRequestHandler<ChangeRadiusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeRadiusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeRadiusCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.ChangeRadius(request.AllowedRadiusMeters);

        return Result.Success();
    }
}

