using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Permissions.Commands;

public class DeletePermissionCommand : IRequest<Result>
{
    public Guid PermissionId { get; set; }
}

public class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}

public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeletePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions.FirstOrDefaultAsync(x => x.Id == request.PermissionId, cancellationToken);
        if (permission == null) return Result.Failure(new[] { "Permission not found." });
        
        permission.Delete();
        _context.Permissions.Update(permission);

        return Result.Success();
    }
}



