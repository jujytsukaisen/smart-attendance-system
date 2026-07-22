using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Permissions.Commands;

public class CreatePermissionCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SystemName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(200);
    }
}

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreatePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = SmartAttendance.Domain.Entities.Permission.Create(
            request.Name,
            request.SystemName,
            request.Module,
            request.Description);
            
        await _context.Permissions.AddAsync(permission, cancellationToken);

        return Result<Guid>.Success(permission.Id);
    }
}

