using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AuditLogs.Queries;

public class GetAuditLogsByEntityQuery : PaginationRequest, IRequest<Result<PagedResult<AuditLogListDto>>>
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
}

public class GetAuditLogsByEntityQueryValidator : AbstractValidator<GetAuditLogsByEntityQuery>
{
    public GetAuditLogsByEntityQueryValidator()
    {
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetAuditLogsByEntityQueryHandler : IRequestHandler<GetAuditLogsByEntityQuery, Result<PagedResult<AuditLogListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAuditLogsByEntityQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AuditLogListDto>>> Handle(GetAuditLogsByEntityQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.Where(x => x.EntityName == request.EntityName).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityId))
        {
            query = query.Where(x => x.EntityId == request.EntityId);
        }

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

