using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Commands;

public class ActivateRoleCommand : IRequest<Result>
{
    public Guid RoleId { get; set; }
}

public class ActivateRoleCommandValidator : AbstractValidator<ActivateRoleCommand>
{
    public ActivateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class ActivateRoleCommandHandler : IRequestHandler<ActivateRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);
        if (role == null) return Result.Failure(new[] { "Role not found." });
        
        role.Activate();
        _context.Roles.Update(role);

        return Result.Success();
    }
}

