using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Commands;

public class UpdateRoleCommand : IRequest<Result>
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(200);
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);
        if (role == null) return Result.Failure(new[] { "Role not found." });
        
        role.Rename(request.Name);
        role.ChangeDescription(request.Description);
        
        _context.Roles.Update(role);

        return Result.Success();
    }
}



