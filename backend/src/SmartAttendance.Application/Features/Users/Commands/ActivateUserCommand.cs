using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class ActivateUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}

public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new[] { "User not found." });
        user.Activate();
        _context.Users.Update(user);

        return Result.Success();
    }
}



