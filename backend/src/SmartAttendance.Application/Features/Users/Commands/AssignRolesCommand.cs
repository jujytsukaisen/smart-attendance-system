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

public class AssignRolesCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public class AssignRolesCommandHandler : IRequestHandler<AssignRolesCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public AssignRolesCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

        public async Task<Result> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new[] { "User not found." });
        foreach (var roleName in request.Roles)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name == roleName, cancellationToken);
            if (role != null) user.AssignRole(role);
        }
        _context.Users.Update(user);

        return Result.Success();
    }
}



