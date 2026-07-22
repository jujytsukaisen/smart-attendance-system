using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class MarkAsHeadOfficeCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
}

public class MarkAsHeadOfficeCommandValidator : AbstractValidator<MarkAsHeadOfficeCommand>
{
    public MarkAsHeadOfficeCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class MarkAsHeadOfficeCommandHandler : IRequestHandler<MarkAsHeadOfficeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsHeadOfficeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsHeadOfficeCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.MarkAsHeadOffice();

        return Result.Success();
    }
}

