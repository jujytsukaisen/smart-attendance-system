using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Commands;

public class AssignPermissionsCommand : IRequest<Result>
{
    public Guid RoleId { get; set; }
    /// <summary>Permission IDs as strings (GUIDs). Sent from frontend as string array.</summary>
    public List<string> PermissionIds { get; set; } = new();
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

        // Parse string GUIDs — skip any invalid values gracefully
        var guids = request.PermissionIds
            .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        // Load target permissions in a single query (avoid IQueryable + in-memory mix)
        var permissions = await _context.Permissions
            .Where(p => guids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        role.ReplacePermissions(permissions);

        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
