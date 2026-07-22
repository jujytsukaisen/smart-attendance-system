using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class ChangeUsernameCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string NewUsername { get; set; } = string.Empty;
}

public class ChangeUsernameCommandValidator : AbstractValidator<ChangeUsernameCommand>
{
    public ChangeUsernameCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewUsername).NotEmpty().MaximumLength(50);
    }
}

public class ChangeUsernameCommandHandler : IRequestHandler<ChangeUsernameCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeUsernameCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

        public async Task<Result> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new[] { "User not found." });
        user.ChangeUsername(request.NewUsername);
        _context.Users.Update(user);

        return Result.Success();
    }
}



