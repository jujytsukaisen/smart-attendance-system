using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Queries;

public class SearchLeavesQuery : PaginationRequest, IRequest<Result<PagedResult<LeaveListDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchLeavesQueryValidator : AbstractValidator<SearchLeavesQuery>
{
    public SearchLeavesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class SearchLeavesQueryHandler : IRequestHandler<SearchLeavesQuery, Result<PagedResult<LeaveListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchLeavesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<LeaveListDto>>> Handle(SearchLeavesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leaves.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Reason.Contains(request.SearchTerm));
        }

        var count = await query.CountAsync(cancellationToken);

        var leaves = await query
            .OrderByDescending(x => x.RequestedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<LeaveListDto>>(leaves);
        
        var result = new PagedResult<LeaveListDto>
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

        return Result<PagedResult<LeaveListDto>>.Success(result);
    }
}

