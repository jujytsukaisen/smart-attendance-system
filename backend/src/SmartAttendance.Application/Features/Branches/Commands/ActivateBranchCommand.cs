using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class ActivateBranchCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
}

public class ActivateBranchCommandValidator : AbstractValidator<ActivateBranchCommand>
{
    public ActivateBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class ActivateBranchCommandHandler : IRequestHandler<ActivateBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.Activate();

        return Result.Success();
    }
}

