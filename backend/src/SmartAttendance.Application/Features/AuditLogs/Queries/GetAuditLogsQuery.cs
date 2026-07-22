using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.AuditLogs.Queries;

public class AuditLogListDto : IMapFrom<AuditLog>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<AuditLog, AuditLogListDto>();
    }
}

public class GetAuditLogsQuery : PaginationRequest, IRequest<Result<PagedResult<AuditLogListDto>>>
{
}

public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAuditLogsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AuditLogListDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        var count = await query.CountAsync(cancellationToken);

        var logs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<AuditLogListDto>>(logs);
        
        var result = new PagedResult<AuditLogListDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse 
            {
                TotalCount = count,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(count / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)System.Math.Ceiling(count / (double)request.PageSize)
            }
        };

        return Result<PagedResult<AuditLogListDto>>.Success(result);
    }
}

