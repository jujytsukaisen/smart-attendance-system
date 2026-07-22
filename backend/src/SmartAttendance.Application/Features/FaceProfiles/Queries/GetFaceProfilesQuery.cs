using FluentValidation;
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

namespace SmartAttendance.Application.Features.FaceProfiles.Queries;

public class GetFaceProfilesQuery : PaginationRequest, IRequest<Result<PagedResult<FaceProfileListDto>>>
{
}

public class FaceProfileListDto : IMapFrom<FaceProfile>
{
    public System.Guid Id { get; set; }
    public System.Guid EmployeeId { get; set; }
    public string FaceId { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<FaceProfile, FaceProfileListDto>();
    }
}

public class GetFaceProfilesQueryValidator : AbstractValidator<GetFaceProfilesQuery>
{
    public GetFaceProfilesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetFaceProfilesQueryHandler : IRequestHandler<GetFaceProfilesQuery, Result<PagedResult<FaceProfileListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFaceProfilesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<FaceProfileListDto>>> Handle(GetFaceProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FaceProfiles.AsNoTracking().AsQueryable();

        var count = await query.CountAsync(cancellationToken);

        var profiles = await query
            .OrderBy(x => x.RegisteredAt)
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

