using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Commands;

public class CloneRoleCommand : IRequest<Result<Guid>>
{
    public Guid SourceRoleId { get; set; }
    public string NewRoleName { get; set; } = string.Empty;
}

public class CloneRoleCommandValidator : AbstractValidator<CloneRoleCommand>
{
    public CloneRoleCommandValidator()
    {
        RuleFor(x => x.SourceRoleId).NotEmpty();
        RuleFor(x => x.NewRoleName).NotEmpty().MaximumLength(50);
    }
}

public class CloneRoleCommandHandler : IRequestHandler<CloneRoleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CloneRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CloneRoleCommand request, CancellationToken cancellationToken)
    {
        var sourceRole = await _context.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == request.SourceRoleId, cancellationToken);
        if (sourceRole == null) return Result<Guid>.Failure(new[] { "Source role not found." });
        
        var newCode = request.NewRoleName.Replace(" ", "_").ToUpper();
        var newRole = SmartAttendance.Domain.Entities.Role.Create(sourceRole.CompanyId, request.NewRoleName, newCode, sourceRole.Description);
        
        foreach (var p in sourceRole.Permissions)
        {
            newRole.AddPermission(p);
        }
        
        await _context.Roles.AddAsync(newRole, cancellationToken);

        return Result<Guid>.Success(newRole.Id);
    }
}

