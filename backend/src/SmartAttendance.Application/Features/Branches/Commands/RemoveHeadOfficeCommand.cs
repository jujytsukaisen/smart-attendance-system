using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class RemoveHeadOfficeCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
}

public class RemoveHeadOfficeCommandValidator : AbstractValidator<RemoveHeadOfficeCommand>
{
    public RemoveHeadOfficeCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class RemoveHeadOfficeCommandHandler : IRequestHandler<RemoveHeadOfficeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveHeadOfficeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveHeadOfficeCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.RemoveHeadOffice();

        return Result.Success();
    }
}

