using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class DeactivateBranchCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
}

public class DeactivateBranchCommandValidator : AbstractValidator<DeactivateBranchCommand>
{
    public DeactivateBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class DeactivateBranchCommandHandler : IRequestHandler<DeactivateBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.Deactivate();

        return Result.Success();
    }
}

