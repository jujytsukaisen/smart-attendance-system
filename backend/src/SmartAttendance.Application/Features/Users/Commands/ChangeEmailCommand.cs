using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class ChangeEmailCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; } = string.Empty;
}

public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    public ChangeEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress();
    }
}

public class ChangeEmailCommandHandler : IRequestHandler<ChangeEmailCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

        public async Task<Result> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new[] { "User not found." });
        var emailResult = new SmartAttendance.Domain.ValueObjects.Email(request.NewEmail);
        user.ChangeEmail(emailResult);
        _context.Users.Update(user);

        return Result.Success();
    }
}



