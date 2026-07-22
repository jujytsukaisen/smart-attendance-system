using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

using SmartAttendance.Application.Features.AttendancePolicies.DTOs;

namespace SmartAttendance.Application.Features.AttendancePolicies.Queries;

public class GetAttendancePoliciesQuery : PaginationRequest, IRequest<Result<PagedResult<AttendancePolicyListDto>>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class GetAttendancePoliciesQueryValidator : AbstractValidator<GetAttendancePoliciesQuery>
{
    public GetAttendancePoliciesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetAttendancePoliciesQueryHandler : IRequestHandler<GetAttendancePoliciesQuery, Result<PagedResult<AttendancePolicyListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAttendancePoliciesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AttendancePolicyListDto>>> Handle(GetAttendancePoliciesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AttendancePolicies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.SearchTerm) || 
                                     p.Code.Contains(request.SearchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "code" => request.SortDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            _ => request.SortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        var policyEntities = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var policies = _mapper.Map<List<AttendancePolicyListDto>>(policyEntities);

        var totalPages = (int)System.Math.Ceiling(totalCount / (double)request.PageSize);
        var result = new PagedResult<AttendancePolicyListDto>
        {
            Items = policies,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                HasNextPage = request.PageNumber < totalPages,
                HasPreviousPage = request.PageNumber > 1
            }
        };

        return Result<PagedResult<AttendancePolicyListDto>>.Success(result);
    }
}
