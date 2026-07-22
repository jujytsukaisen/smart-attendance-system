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

using SmartAttendance.Application.Features.Departments.DTOs;

namespace SmartAttendance.Application.Features.Departments.Queries;

public class GetDepartmentsQuery : PaginationRequest, IRequest<Result<PagedResult<DepartmentListDto>>>
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class GetDepartmentsQueryValidator : AbstractValidator<GetDepartmentsQuery>
{
    public GetDepartmentsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, Result<PagedResult<DepartmentListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDepartmentsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<DepartmentListDto>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Departments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var cleanSearchTerm = request.SearchTerm.Trim();
            query = query.Where(d => 
                d.Name.Contains(cleanSearchTerm) || 
                d.Code.Contains(cleanSearchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "code" => request.SortDescending ? query.OrderByDescending(d => d.Code) : query.OrderBy(d => d.Code),
            _ => query.OrderBy(d => d.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<DepartmentListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<DepartmentListDto>
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

        return Result<PagedResult<DepartmentListDto>>.Success(result);
    }
}
