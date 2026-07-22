using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AuditLogs.Commands;

public class CreateAuditLogCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string OldValues { get; set; } = string.Empty;
    public string NewValues { get; set; } = string.Empty;
}

public class CreateAuditLogCommandValidator : AbstractValidator<CreateAuditLogCommand>
{
    public CreateAuditLogCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(100);
    }
}

public class CreateAuditLogCommandHandler : IRequestHandler<CreateAuditLogCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateAuditLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        var action = Enum.TryParse<SmartAttendance.Domain.Enums.AuditAction>(request.Action, out var parsedAction) 
            ? parsedAction 
            : SmartAttendance.Domain.Enums.AuditAction.Other;
            
        var log = SmartAttendance.Domain.Entities.AuditLog.Create(
            request.UserId,
            request.EntityName,
            request.EntityId,
            action,
            request.OldValues,
            request.NewValues);
            
        await _context.AuditLogs.AddAsync(log, cancellationToken);

        return Result<Guid>.Success(log.Id);
    }
}

