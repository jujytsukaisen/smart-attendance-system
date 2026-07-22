using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class RestoreBranchCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
}

public class RestoreBranchCommandValidator : AbstractValidator<RestoreBranchCommand>
{
    public RestoreBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class RestoreBranchCommandHandler : IRequestHandler<RestoreBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.Restore();

        return Result.Success();
    }
}
