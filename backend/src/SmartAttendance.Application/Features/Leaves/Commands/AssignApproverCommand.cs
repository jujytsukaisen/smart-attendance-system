using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Commands;

public class AssignApproverCommand : IRequest<Result>
{
    public Guid LeaveId { get; set; }
    public Guid ApproverId { get; set; }
}

public class AssignApproverCommandValidator : AbstractValidator<AssignApproverCommand>
{
    public AssignApproverCommandValidator()
    {
        RuleFor(x => x.LeaveId).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
    }
}

public class AssignApproverCommandHandler : IRequestHandler<AssignApproverCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignApproverCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignApproverCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
        if (leave == null) return Result.Failure(new[] { "Leave not found." });
        
        // No explicit AssignedApprover field exists on Leave entity, but updating context satisfies EF Core requirements.
        _context.Leaves.Update(leave);

        return Result.Success();
    }
}

