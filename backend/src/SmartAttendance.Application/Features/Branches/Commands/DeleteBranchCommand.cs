using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class DeleteBranchCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
}

public class DeleteBranchCommandValidator : AbstractValidator<DeleteBranchCommand>
{
    public DeleteBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.Delete();

        return Result.Success();
    }
}



