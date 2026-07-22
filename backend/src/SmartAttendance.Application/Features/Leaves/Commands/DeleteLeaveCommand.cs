using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Commands;

public class DeleteLeaveCommand : IRequest<Result>
{
    public Guid LeaveId { get; set; }
}

public class DeleteLeaveCommandValidator : AbstractValidator<DeleteLeaveCommand>
{
    public DeleteLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveId).NotEmpty();
    }
}

public class DeleteLeaveCommandHandler : IRequestHandler<DeleteLeaveCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteLeaveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
        if (leave == null) return Result.Failure(new[] { "Leave not found." });
        
        _context.Leaves.Remove(leave);

        return Result.Success();
    }
}



