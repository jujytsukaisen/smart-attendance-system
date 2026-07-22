using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Commands;

public class DeleteRoleCommand : IRequest<Result>
{
    public Guid RoleId { get; set; }
}

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);
        if (role == null) return Result.Failure(new[] { "Role not found." });
        
        role.Delete();
        _context.Roles.Update(role);

        return Result.Success();
    }
}



