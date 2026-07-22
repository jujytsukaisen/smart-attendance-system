using FluentValidation;
using System;
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

namespace SmartAttendance.Application.Features.Leaves.Queries;

public class LeaveDto : IMapFrom<Leave>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Leave, LeaveDto>();
    }
}

public class GetLeavesByEmployeeQuery : PaginationRequest, IRequest<Result<PagedResult<LeaveDto>>>
{
    public Guid EmployeeId { get; set; }
}

public class GetLeavesByEmployeeQueryValidator : AbstractValidator<GetLeavesByEmployeeQuery>
{
    public GetLeavesByEmployeeQueryValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetLeavesByEmployeeQueryHandler : IRequestHandler<GetLeavesByEmployeeQuery, Result<PagedResult<LeaveDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLeavesByEmployeeQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<LeaveDto>>> Handle(GetLeavesByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leaves.Where(x => x.EmployeeId == request.EmployeeId).AsNoTracking().AsQueryable();

        var count = await query.CountAsync(cancellationToken);

        var leaves = await query
            .OrderByDescending(x => x.RequestedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<LeaveDto>>(leaves);
        
        var result = new PagedResult<LeaveDto>
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

        return Result<PagedResult<LeaveDto>>.Success(result);
    }
}

