using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Permissions.Commands;

public class UpdatePermissionCommand : IRequest<Result>
{
    public Guid PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(200);
    }
}

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdatePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions.FirstOrDefaultAsync(x => x.Id == request.PermissionId, cancellationToken);
        if (permission == null) return Result.Failure(new[] { "Permission not found." });
        
        permission.Rename(request.Name);
        permission.ChangeDescription(request.Description);
        
        _context.Permissions.Update(permission);

        return Result.Success();
    }
}



