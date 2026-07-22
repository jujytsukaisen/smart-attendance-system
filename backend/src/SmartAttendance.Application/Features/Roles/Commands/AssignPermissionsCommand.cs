using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Commands;

public class AssignPermissionsCommand : IRequest<Result>
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}

public class AssignPermissionsCommandValidator : AbstractValidator<AssignPermissionsCommand>
{
    public AssignPermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionIds).NotEmpty();
    }
}

public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignPermissionsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);
        if (role == null) return Result.Failure(new[] { "Role not found." });
        
        var permissions = await _context.Permissions.Where(p => request.PermissionIds.Contains(p.Id)).ToListAsync(cancellationToken);
        foreach (var p in permissions)
        {
            if (!role.Permissions.Any(x => x.Id == p.Id))
            {
                role.AddPermission(p);
            }
        }
        
        _context.Roles.Update(role);

        return Result.Success();
    }
}

