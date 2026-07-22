using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Queries;

public class SearchFaceProfilesQuery : PaginationRequest, IRequest<Result<PagedResult<FaceProfileListDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchFaceProfilesQueryValidator : AbstractValidator<SearchFaceProfilesQuery>
{
    public SearchFaceProfilesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class SearchFaceProfilesQueryHandler : IRequestHandler<SearchFaceProfilesQuery, Result<PagedResult<FaceProfileListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchFaceProfilesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<FaceProfileListDto>>> Handle(SearchFaceProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FaceProfiles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // Face profile ID isn't exactly searchable by name, maybe filter by EmployeeId if it was string, but we'll do simple filter for FaceProfileId
            query = query.Where(x => x.FaceProfileId != null && x.FaceProfileId.Contains(request.SearchTerm));
        }

        var count = await query.CountAsync(cancellationToken);

        var profiles = await query
            .OrderByDescending(x => x.RegisteredAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<FaceProfileListDto>>(profiles);
        
        var result = new PagedResult<FaceProfileListDto>
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

        return Result<PagedResult<FaceProfileListDto>>.Success(result);
    }
}

