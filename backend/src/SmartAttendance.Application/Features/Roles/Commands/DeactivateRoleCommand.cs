using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Commands;

public class DeactivateRoleCommand : IRequest<Result>
{
    public Guid RoleId { get; set; }
}

public class DeactivateRoleCommandValidator : AbstractValidator<DeactivateRoleCommand>
{
    public DeactivateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class DeactivateRoleCommandHandler : IRequestHandler<DeactivateRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);
        if (role == null) return Result.Failure(new[] { "Role not found." });
        
        role.Deactivate();
        _context.Roles.Update(role);

        return Result.Success();
    }
}

