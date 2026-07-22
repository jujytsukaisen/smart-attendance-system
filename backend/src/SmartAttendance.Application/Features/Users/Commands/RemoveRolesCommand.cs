using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class RemoveRolesCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class RemoveRolesCommandValidator : AbstractValidator<RemoveRolesCommand>
{
    public RemoveRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public class RemoveRolesCommandHandler : IRequestHandler<RemoveRolesCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public RemoveRolesCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

        public async Task<Result> Handle(RemoveRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new[] { "User not found." });
        foreach (var roleName in request.Roles)
        {
            var role = user.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role != null) user.RemoveRole(role);
        }
        _context.Users.Update(user);

        return Result.Success();
    }
}



