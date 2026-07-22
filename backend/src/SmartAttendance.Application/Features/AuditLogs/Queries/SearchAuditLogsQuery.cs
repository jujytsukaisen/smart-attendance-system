using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AuditLogs.Queries;

public class SearchAuditLogsQuery : PaginationRequest, IRequest<Result<PagedResult<AuditLogListDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchAuditLogsQueryValidator : AbstractValidator<SearchAuditLogsQuery>
{
    public SearchAuditLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class SearchAuditLogsQueryHandler : IRequestHandler<SearchAuditLogsQuery, Result<PagedResult<AuditLogListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchAuditLogsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AuditLogListDto>>> Handle(SearchAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.EntityName.Contains(request.SearchTerm) || x.EntityId.Contains(request.SearchTerm) || (x.OldValues != null && x.OldValues.Contains(request.SearchTerm)) || (x.NewValues != null && x.NewValues.Contains(request.SearchTerm)));
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

