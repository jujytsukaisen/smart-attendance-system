using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.AuditLogs.Queries;

public class AuditLogDetailsDto : IMapFrom<AuditLog>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string OldValues { get; set; } = string.Empty;
    public string NewValues { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsArchived { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<AuditLog, AuditLogDetailsDto>();
    }
}

public class GetAuditLogByIdQuery : IRequest<Result<AuditLogDetailsDto>>
{
    public Guid AuditLogId { get; set; }
}

public class GetAuditLogByIdQueryValidator : AbstractValidator<GetAuditLogByIdQuery>
{
    public GetAuditLogByIdQueryValidator()
    {
        RuleFor(x => x.AuditLogId).NotEmpty();
    }
}

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAuditLogByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AuditLogDetailsDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AuditLogId, cancellationToken);
            
        if (log == null) return Result<AuditLogDetailsDto>.Failure(new[] { "Audit log not found." });
        
        var dto = _mapper.Map<AuditLogDetailsDto>(log);
        
        return Result<AuditLogDetailsDto>.Success(dto);
    }
}

