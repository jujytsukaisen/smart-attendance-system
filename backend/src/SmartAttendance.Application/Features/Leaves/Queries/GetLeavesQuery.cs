using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Leaves.Queries;

public class LeaveListDto : IMapFrom<Leave>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Leave, LeaveListDto>();
    }
}

public class GetLeavesQuery : PaginationRequest, IRequest<Result<PagedResult<LeaveListDto>>>
{
}

public class GetLeavesQueryValidator : AbstractValidator<GetLeavesQuery>
{
    public GetLeavesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetLeavesQueryHandler : IRequestHandler<GetLeavesQuery, Result<PagedResult<LeaveListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLeavesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<LeaveListDto>>> Handle(GetLeavesQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_context.Leaves), cancellationToken);
        var leaves = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            System.Linq.Queryable.Take(
                System.Linq.Queryable.Skip(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_context.Leaves), (request.PageNumber - 1) * request.PageSize),
                request.PageSize),
            cancellationToken);

        var dtos = _mapper.Map<System.Collections.Generic.List<LeaveListDto>>(leaves);
        
        var result = new PagedResult<LeaveListDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse 
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)System.Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };
        
        return Result<PagedResult<LeaveListDto>>.Success(result);
    }
}
