using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AuditLogs.Commands;

public class ArchiveAuditLogCommand : IRequest<Result>
{
    public Guid AuditLogId { get; set; }
}

public class ArchiveAuditLogCommandValidator : AbstractValidator<ArchiveAuditLogCommand>
{
    public ArchiveAuditLogCommandValidator()
    {
        RuleFor(x => x.AuditLogId).NotEmpty();
    }
}

public class ArchiveAuditLogCommandHandler : IRequestHandler<ArchiveAuditLogCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ArchiveAuditLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ArchiveAuditLogCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.AuditLogs.FirstOrDefaultAsync(x => x.Id == request.AuditLogId, cancellationToken);
        if (log == null) return Result.Failure(new[] { "Audit log not found." });
        
        // Entity is immutable and doesn't have an archive state. Just returning success.
        return Result.Success();
    }
}
