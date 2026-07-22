using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class SuspendUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}

public class SuspendUserCommandValidator : AbstractValidator<SuspendUserCommand>
{
    public SuspendUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SuspendUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

        public async Task<Result> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new[] { "User not found." });
        user.Suspend();
        _context.Users.Update(user);

        return Result.Success();
    }
}



