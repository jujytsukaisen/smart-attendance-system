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

namespace SmartAttendance.Application.Features.Leaves.Queries;

public class GetPendingLeavesQuery : PaginationRequest, IRequest<Result<PagedResult<LeaveListDto>>>
{
    public Guid ApproverId { get; set; }
}

public class GetPendingLeavesQueryValidator : AbstractValidator<GetPendingLeavesQuery>
{
    public GetPendingLeavesQueryValidator()
    {
        RuleFor(x => x.ApproverId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetPendingLeavesQueryHandler : IRequestHandler<GetPendingLeavesQuery, Result<PagedResult<LeaveListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPendingLeavesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<LeaveListDto>>> Handle(GetPendingLeavesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leaves.Where(x => x.LeaveStatus == SmartAttendance.Domain.Enums.LeaveStatus.Pending).AsNoTracking().AsQueryable();

        var count = await query.CountAsync(cancellationToken);

        var leaves = await query
            .OrderBy(x => x.RequestedAt)
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

