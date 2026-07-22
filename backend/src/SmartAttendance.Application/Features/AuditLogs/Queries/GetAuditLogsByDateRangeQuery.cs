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

public class GetAuditLogsByDateRangeQuery : PaginationRequest, IRequest<Result<PagedResult<AuditLogListDto>>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetAuditLogsByDateRangeQueryValidator : AbstractValidator<GetAuditLogsByDateRangeQuery>
{
    public GetAuditLogsByDateRangeQueryValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetAuditLogsByDateRangeQueryHandler : IRequestHandler<GetAuditLogsByDateRangeQuery, Result<PagedResult<AuditLogListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAuditLogsByDateRangeQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AuditLogListDto>>> Handle(GetAuditLogsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.Where(x => x.CreatedAt >= request.StartDate && x.CreatedAt <= request.EndDate).AsNoTracking().AsQueryable();

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

