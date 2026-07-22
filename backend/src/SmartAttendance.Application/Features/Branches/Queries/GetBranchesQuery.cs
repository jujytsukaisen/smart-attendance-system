using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

using SmartAttendance.Application.Features.Branches.DTOs;

namespace SmartAttendance.Application.Features.Branches.Queries;

public class GetBranchesQuery : PaginationRequest, IRequest<Result<PagedResult<BranchListDto>>>
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class GetBranchesQueryValidator : AbstractValidator<GetBranchesQuery>
{
    public GetBranchesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, Result<PagedResult<BranchListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<BranchListDto>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Branches.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            query = query.Where(b => b.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim();
            query = query.Where(b => b.Code.Contains(code));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(b => b.IsActive == request.IsActive.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
            "code" => request.SortDescending ? query.OrderByDescending(b => b.Code) : query.OrderBy(b => b.Code),
            _ => request.SortDescending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<BranchListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<BranchListDto>
        {
            Items = items,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            }
        };

        return Result<PagedResult<BranchListDto>>.Success(result);
    }
}
