using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Roles.Queries;

public class SearchRolesQuery : PaginationRequest, IRequest<Result<PagedResult<RoleDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchRolesQueryValidator : AbstractValidator<SearchRolesQuery>
{
    public SearchRolesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class SearchRolesQueryHandler : IRequestHandler<SearchRolesQuery, Result<PagedResult<RoleDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchRolesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<RoleDto>>> Handle(SearchRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Roles.Include(x => x.Permissions).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Name.Contains(request.SearchTerm) || (x.Description != null && x.Description.Contains(request.SearchTerm)));
        }

        var count = await query.CountAsync(cancellationToken);

        var roles = await query
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<RoleDto>>(roles);
        
        var result = new PagedResult<RoleDto>
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

        return Result<PagedResult<RoleDto>>.Success(result);
    }
}

