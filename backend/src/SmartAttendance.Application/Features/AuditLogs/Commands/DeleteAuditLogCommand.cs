using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AuditLogs.Commands;

public class DeleteAuditLogCommand : IRequest<Result>
{
    public Guid AuditLogId { get; set; }
}

public class DeleteAuditLogCommandValidator : AbstractValidator<DeleteAuditLogCommand>
{
    public DeleteAuditLogCommandValidator()
    {
        RuleFor(x => x.AuditLogId).NotEmpty();
    }
}

public class DeleteAuditLogCommandHandler : IRequestHandler<DeleteAuditLogCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteAuditLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteAuditLogCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.AuditLogs.FirstOrDefaultAsync(x => x.Id == request.AuditLogId, cancellationToken);
        if (log == null) return Result.Failure(new[] { "Audit log not found." });
        
        _context.AuditLogs.Remove(log);

        return Result.Success();
    }
}



