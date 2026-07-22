using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new[] { "User not found." });
        
        var email = new SmartAttendance.Domain.ValueObjects.Email(request.Email);
        user.ChangeEmail(email);
        user.ChangeUsername(request.Username);

        _context.Users.Update(user);
        // Note: SaveChangesAsync will be removed in H6 later, but I'll remove it now to adhere to H6.
        // Wait, H6 says "Handlers never call SaveChangesAsync OR Pipeline never calls CommitAsync."
        // Our plan is to remove SaveChangesAsync from handlers. So I won't call it here.
        
        return Result.Success();
    }
}




